using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using BooksTextsSplit.Library.BookAnalysis;
using BooksTextsSplit.Library.Models;

namespace BooksTextsSplit.Library.Services
{
    public interface IBackgroundTasksService
    {
        void BackgroundRecordBookToDb(IFormFile bookFile, string jsonBookDescription, string guid);
        void WorkerSample(string guid);
    }
    public class BackgroundTasksService : IBackgroundTasksService
    {
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly ILogger<BackgroundTasksService> _logger;
        private readonly ISettingConstants _constant;
        private readonly IControllerDataManager _data;
        private readonly IAccessCacheData _access;
        private readonly ICosmosDbService _context;

        public BackgroundTasksService(
            IBackgroundTaskQueue taskQueue,
            ILogger<BackgroundTasksService> logger,
            ISettingConstants constant,
            IControllerDataManager data,
            ICosmosDbService cosmosDbService,
            IAccessCacheData access)
        {
            _taskQueue = taskQueue;
            _logger = logger;
            _constant = constant;
            _data = data;
            _access = access;
            _context = cosmosDbService;
        }

        private void BackgroundUpdateKeys(string taskGuid)
        {
            // Enqueue a background work item
            _taskQueue.QueueBackgroundWorkItem(async token =>
            {
                try
                {
                    _logger.LogInformation("Queued Background Task UpdateKeys {Guid} is starting", taskGuid);
                    TaskUploadPercents updateTaskState = _data.CreateTaskGuidPercentsKeys(taskGuid);
                    updateTaskState.IsTaskRunning = true;
                    bool resultSetKey = await _data.SetTaskState(updateTaskState, 3);
                }
                catch (OperationCanceledException)
                {
                    // Prevent throwing if the Delay is cancelled
                }
            });
        }

        public void BackgroundRecordBookToDb(IFormFile bookFile, string jsonBookDescription, string guid)
        {
            JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, };
            TextSentence bookDescription = JsonSerializer.Deserialize<TextSentence>(jsonBookDescription, options);
            int desiredTextLanguage = bookDescription.LanguageId;

            // Define the cancellation token for Task.Delay
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken tokenDelay = source.Token;

            // move to FetchBookTextSentences?
            string fileName = bookFile.FileName;
            StreamReader reader = new StreamReader(bookFile.OpenReadStream());
            string text = reader.ReadToEnd();

            TextSentence[] textSentences = FetchBookTextSentences(text, bookDescription, desiredTextLanguage);
            int textSentencesLength = textSentences.Length;

            // Enqueue a background work item
            _taskQueue.QueueBackgroundWorkItem(async token =>
            {
                TaskUploadPercents isExistBackgroundUpdateKeys = await _data.IsExistBackgroundUpdateKeys();
                if (!isExistBackgroundUpdateKeys.IsTaskRunning)
                {
                    // BackgroundUpdateKeys(isExistBackgroundUpdateKeys.CurrentTaskGuid);
                }

                try
                {
                    _logger.LogInformation("Queued Background Task RecordFileToDb {Guid} is starting", guid);

                    TaskUploadPercents uploadPercents = _data.CreateTaskGuidPercentsKeys(guid, bookDescription, textSentencesLength);
                    uploadPercents.IsTaskRunning = true;  //inform all the task is started
                    bool resultSetKey = await _data.SetTaskState(uploadPercents);

                    try
                    {
                        // порядок действий -
                        // проверяем наличие ключа таблиц (bookTableKey)
                        // если его нет, создаём всю таблицу, но потом, после записи глав
                        // сначала создаём List ключей глав
                        // записываем в него ключи глав в цикле tsi
                        // создаём List версий
                        // создаём таблицу
                        // записываем её в ключ таблиц в поле bookId
                        // - если ключ есть, проверяем, есть ли в нем поле bookId
                        // - если нет, создаём всю таблицу, как описано выше
                        // -- если такое поле есть, достаем из него таблицу (как уже сделано)
                        // -- из таблицы достаем List версий
                        // -- проверяем есть ли текущая версия
                        // -- если нет, создаём List ключей глав, заполняем его, как указано выше (в цикле tsi)
                        // --- если такая версия есть, достаем из неё List ключей глав
                        // --- тут ещё можно проверить совпадение количества глав - на всякий случай
                        // --- что делать, если не совпадают, непонятно, просто сообщить
                        // --- в цикле отключаем генерацию ключей глав, берём ключи из List
                        // --- после записи всех глав ничего делать не надо, таблица не меняется
                        // -- после записи глав создаём новый элемент List версий, вставляем в него созданный List ключей глав, записываем обновленную таблицу в поле bookId
                        // - создаём List версий, создаём новый элемент List версий, вставляем в него созданный List ключей глав, создаём таблицу, записываем в поле bookId


                        TextSentence chapterContext = textSentences[0];
                        string recordGuid = chapterContext.Id;
                        int bookId = chapterContext.BookId;
                        int recordActualityLevel = chapterContext.RecordActualityLevel;
                        int uploadVersion = chapterContext.UploadVersion;
                        int languageId = chapterContext.LanguageId;

                        // проверить, есть ли такой bookId в ключе таблиц
                        BookTable bookTable = await _data.CheckBookId(bookId, uploadVersion, recordActualityLevel);

                        //List<BookTable.UploadVersionContent.ChaptersPair> chaptersPairKeys = bookTable.UploadVersions;

                        _logger.LogInformation($"Queued Background Task RecordFileToDb {guid} is running", guid);

                        for (int tsi = 0; tsi < textSentencesLength; tsi++)
                        {
                            // Check the time of one cycle, calculate the whole task run time, if it is more 10 sec, than percents will be shown - 1 state per second
                            Stopwatch stopWatch = new Stopwatch();
                            stopWatch.Start();

                            if (textSentencesLength < 20)
                            {
                                await Task.Delay(_constant.GetTaskDelayTimeInSeconds * 1000, tokenDelay); // delay to emulate upload of a real book - 
                            }

                            await _context.AddItemAsync(textSentences[tsi]); // возвращать значение charges - если не нулевое, значит что-то записалось

                            // каждая пара глав (eng-rus) хранится в отдельном ключе guid
                            // все главы пары книг собираются в лист и хранятся в таблице, которая хранится в общем ключе с полем bookId
                            // проверить, есть ли такая книга на другом языке, если есть, guid брать готовые


                            string currentChapterKey = Guid.NewGuid().ToString();
                            chaptersPairKeys.Add(new BookTable.UploadVersionContent.ChaptersPair
                            {
                                ChaptersKey = currentChapterKey,
                                ChapterNumber = tsi
                            });


                            await _data.AddChapter(currentChapterKey, languageId, textSentences[tsi]);


                            stopWatch.Stop();
                            TimeSpan ts = stopWatch.Elapsed; // Get the elapsed time as a TimeSpan value.
                            bool resultSetKey1 = await SetTaskState(uploadPercents, ts, tsi);
                        };





                        // ключ guid создавать через хэш
                        bool removingResult = await UpdateKeysAfterRecording(desiredTextLanguage, guid);

                        // здесь проверить, что задание последнее и если да, то восстановить ключи данных
                        // to create sentenceCounts for current language
                        await _data.TotalRecordsCountWhereLanguageId(desiredTextLanguage);
                        //int totalLangSentences = await _data.FetchDataFromCache(desiredTextLanguage) ?? 0;

                        _logger.LogInformation($"Queued Background Task RecordFileToDb {guid} recorded {textSentencesLength} records to DB");
                    }
                    catch (Exception ex)
                    {
                        string message = $"Queued Background Task RecordFileToDb {guid} is crashed. \n {ex.Message} \n";
                        _logger.LogInformation(message, guid, ex.Message);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Prevent throwing if the Delay is cancelled
                }
            });
            //return "Task " + guid + " was Queued Background";
        }

        private async Task<bool> SetTaskState(TaskUploadPercents uploadPercents, TimeSpan ts, int tsi)
        {
            int tsMs = ts.Milliseconds;
            uploadPercents.CurrentUploadingRecord = tsi;
            uploadPercents.CurrentUploadedRecordRealTime = tsMs;
            uploadPercents.TotalUploadedRealTime += tsMs;
            return await _data.SetTaskState(uploadPercents);
        }

        private async Task<bool> UpdateKeysAfterRecording(int languageId, string guid)
        {
            // здесь проверить наличие ключей других процессов и параметр isRunning в них
            // ещё можно проверять с каким языком процесс, но пока непонятно зачем
            // update key with all bookIds in all languages with all versions
            await _data.FetchBooksNamesVersionsProperties();

            // TO update key with TotalCount(s) here
            await _data.FetchTotalCounts(languageId);

            _logger.LogInformation("QBTask {Guid} has updated key successfully - {languageId}", guid, languageId.ToString());

            return true;
        }

        private TextSentence[] FetchBookTextSentences(string text, TextSentence bookDescription, int desiredTextLanguage)
        {
            // получение плоского текста из анализатора книги
            TextSentenceFlat[] textSentences = AnalyseTextBook(text, desiredTextLanguage);

            int textSentencesLength = textSentences.Length;
            int inChapterParagraphsCount = 0;
            int inChapterSentencesCount = 0;
            int inBookChapterCount = 0;
            int inBookParagraphsCount = 0;
            int inBookSentencesCount = 0;

            // сохранение плоского текста в структуру типа - одна запись - одна глава            
            List<TextSentence.BookContentInChapters.BookContentInParagraphs> bp = new List<TextSentence.BookContentInChapters.BookContentInParagraphs>();
            List<TextSentence.BookContentInChapters> bc = new List<TextSentence.BookContentInChapters>();
            List<TextSentence> bt = new List<TextSentence>();

            for (int i = 0; i < textSentencesLength - 1; i++) //-------------------------------------------------------
            {
                // если номер главы не изменился и номер абзаца не изменился - продолжаем записывать предложения в тот же bp (всегда записываем предложения в bp)
                TextSentence.BookContentInChapters.BookContentInParagraphs p = new TextSentence.BookContentInChapters.BookContentInParagraphs
                {
                    BookSentenceId = textSentences[i].BookSentenceId,
                    SentenceId = textSentences[i].SentenceId,
                    SentenceText = textSentences[i].SentenceText
                };
                p.InParagraphSentencesCounts++;
                inChapterSentencesCount++;
                inBookSentencesCount++;
                bp.Add(p);
                // если в следующей строке абзац поменяется на новый, записываем текущий в главу и начинаем новый абзац
                if (textSentences[i + 1].ParagraphId > textSentences[i].ParagraphId)
                {
                    bc.Add(new TextSentence.BookContentInChapters
                    {
                        ParagraphId = textSentences[i].ParagraphId,
                        ParagraphName = textSentences[i].ParagraphName,
                        BookContentInParagraph = bp
                    });
                    inChapterParagraphsCount++;
                    inBookParagraphsCount++;
                    bp = new List<TextSentence.BookContentInChapters.BookContentInParagraphs>(); //bp.Clear();
                }
                // если в следующей строке глава поменяется на новую, записываем текущую в TextSentence и начинаем новую главу
                if (textSentences[i + 1].ChapterId > textSentences[i].ChapterId)
                {
                    TextSentence t = new TextSentence
                    {
                        Id = Guid.NewGuid().ToString(),
                        BookId = bookDescription.BookId,
                        //RecordActualityLevel = Constants.RecordActualityLevel, // Model TextSentence ver.6
                        RecordActualityLevel = _constant.GetRecordActualityLevel, // Constants.RecordActualityLevel;
                        BookProperties = new TextSentence.BookPropertiesInLanguage
                        {
                            AuthorNameId = bookDescription.BookProperties.AuthorNameId,
                            AuthorName = bookDescription.BookProperties.AuthorName,
                            BookNameId = bookDescription.BookProperties.BookNameId,
                            BookName = bookDescription.BookProperties.BookName,
                            BookAnnotation = bookDescription.BookProperties.BookAnnotation,
                        },
                        UploadVersion = bookDescription.UploadVersion + 1, // have obtained data about last existed version from UI
                        LanguageId = desiredTextLanguage,
                        RecordId = inBookChapterCount,
                        ChapterId = textSentences[i].ChapterId,
                        ChapterName = textSentences[i].ChapterName,
                        InChapterParagraphsCount = inChapterParagraphsCount,
                        InChapterSentencesCount = inChapterSentencesCount,
                        BookContentInChapter = bc
                    };
                    inBookChapterCount++;
                    bt.Add(t);
                    inChapterParagraphsCount = 0;
                    inChapterSentencesCount = 0;

                    bc = new List<TextSentence.BookContentInChapters>(); //bc.Clear();                                        
                }
            }
            // set TotalBooksCounts Counts
            TextSentence.TotalBooksCounts tc = new TextSentence.TotalBooksCounts
            {
                InBookChaptersCount = inBookChapterCount,
                InBookParagraphsCount = inBookParagraphsCount,
                InBookSentencesCount = inBookSentencesCount
            };
            // можно просуммировать все счётчики по главам и абзацам и сравнить их с общими
            //bt.Select(t => t.TotalBookCounts.InBookChaptersCount = tc.InBookChaptersCount);
            //bt.Select(t => t.TotalBookCounts = tc);
            foreach (TextSentence t in bt)
            {
                //TextSentence.TotalBooksCounts tt = new TextSentence.TotalBooksCounts();
                t.TotalBookCounts = tc;
            }
            return bt.ToArray();
        }

        public TextSentenceFlat[] AnalyseTextBook(string text, int desiredTextLanguage)
        {
            IAllBookData _bookData = new AllBookData();
            ITextAnalysisLogicExtension analysisLogic = new TextAnalysisLogicExtension(_bookData);
            ISentencesDividingAnalysis sentenceAnalyser = new SentencesDividingAnalysis(_bookData, analysisLogic);
            //IAnalysisLogicParagraph paragraphAnalysis = new noneAnalysisLogicParagraph(bookData, msgService, analysisLogic);
            IChapterDividingAnalysis chapterAnalyser = new ChapterDividingAnalysis(_bookData, analysisLogic);
            IAllBookAnalysis bookAnalysis = new AllBookAnalysis(_bookData, analysisLogic, chapterAnalyser, sentenceAnalyser);

            //int desiredTextLanguage = bookDescription.LanguageId;
            //создание нужной инструкции ToDo
            _bookData.SetFileToDo((int)WhatNeedDoWithFiles.AnalyseText, desiredTextLanguage);
            //bookData.SetFilePath(_filePath, desiredTextLanguage);

            //List<TextSentence> requestedSelectResult = await cache.Cache.FetchObjectAsync<List<TextSentence>>(booksPairTextsKey, () => FetchBooksTextsFromDb(where1, where1Value));
            //await cache.Cache.SetObjectAsync(createdKeyNameFromRequest, requestedSelectResult, TimeSpan.FromDays(1));

            string fileContent = text;
            _bookData.SetFileContent(fileContent, desiredTextLanguage);

            // получение плоского текста из анализатора книги
            TextSentenceFlat[] textSentences = bookAnalysis.AnalyseTextBook();

            return textSentences;
        }



        public void WorkerSample(string guid)
        {
            // Enqueue a background work item
            _taskQueue.QueueBackgroundWorkItem(async token =>
            {
                // Simulate three 5-second tasks to complete
                // for each enqueued work item

                int delayLoop = 0;
                //string guid = Guid.NewGuid().ToString();

                //Console.WriteLine(
                //    "Queued Background Task {0} is starting.", guid);
                _logger.LogInformation(
                        "Queued Background Task {Guid} is starting.", guid);

                while (!token.IsCancellationRequested && delayLoop < 3)
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5), token);
                    }
                    catch (OperationCanceledException)
                    {
                        // Prevent throwing if the Delay is cancelled
                    }

                    delayLoop++;

                    //Console.WriteLine(
                    //    "Queued Background Task {0} is running. " +
                    //    "{1}/3", guid, delayLoop);
                    _logger.LogInformation(
                            "Queued Background Task {Guid} is running. " +
                            "{DelayLoop}/3", guid, delayLoop);
                }

                if (delayLoop == 3)
                {
                    //Console.WriteLine(
                    //    "Queued Background Task {0} is complete.", guid);
                    _logger.LogInformation(
                            "Queued Background Task {Guid} is complete.", guid);
                }
                else
                {
                    //Console.WriteLine(
                    //    "Queued Background Task {0} was cancelled.", guid);
                    _logger.LogInformation(
                            "Queued Background Task {Guid} was cancelled.", guid);
                }
            });
        }
    }
}

// RabbitMQ - войти через браузер на http://localhost:15672. По умолчанию, логин и пароль для входа в RabbitMQ Managment Plugin: guest/guest
