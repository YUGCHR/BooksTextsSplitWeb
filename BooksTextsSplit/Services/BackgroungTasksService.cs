using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using BooksTextsSplit.Models;
using System.IO;
using System.Text.Json;
using CachingFramework.Redis;
using Microsoft.Extensions.Logging;
using CachingFramework.Redis.Contracts.Providers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Linq;

namespace BooksTextsSplit.Services
{
    public interface IBackgroungTasksService
    {
        void BackgroundRecordBookToDb(IFormFile bookFile, string jsonBookDescription, string guid);
        void WorkerSample(string guid);
    }
    public class BackgroungTasksService : IBackgroungTasksService
    {
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly ILogger<BackgroungTasksService> _logger;
        private readonly ISettingConstants _constant;
        private readonly IControllerDataManager _data;
        private readonly IAccessCacheData _access;
        private readonly ICosmosDbService _context;

        public BackgroungTasksService(
            IBackgroundTaskQueue taskQueue,
            ILogger<BackgroungTasksService> logger,
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

        public void BackgroundRecordBookToDb(IFormFile bookFile, string jsonBookDescription, string guid)
        {
            JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, };
            TextSentence bookDescription = JsonSerializer.Deserialize<TextSentence>(jsonBookDescription, options);
            int desiredTextLanguage = bookDescription.LanguageId;

            // move to FetchBookTextSentences?
            string fileName = bookFile.FileName;
            StreamReader reader = new StreamReader(bookFile.OpenReadStream());
            string text = reader.ReadToEnd();

            TextSentence[] textSentences = FetchBookTextSentences(text, bookDescription, desiredTextLanguage);
            int textSentencesLength = textSentences.Length;

            int taskDelayTimeInSeconds = _constant.GetTaskDelayTimeInSeconds;
            TimeSpan keysExistingTime = TimeSpan.FromMinutes(_constant.GetPersentsKeysExistingTimeInMinutes); // TO DELETE!

            // Enqueue a background work item
            _taskQueue.QueueBackgroundWorkItem(async token =>
            {
                try
                {
                    _logger.LogInformation("Queued Background Task RecordFileToDb {Guid} is starting", guid);

                    TaskUploadPercents uploadPercents = _data.CreateTaskGuidKeys(guid, bookDescription, textSentencesLength);
                    uploadPercents.IsTaskRunning = true;  //inform all the task is started
                    bool resultSetKey = await _data.SetTaskState(uploadPercents);

                    try
                    {
                        _logger.LogInformation("Queued Background Task RecordFileToDb {Guid} is running", guid);

                        for (int tsi = 0; tsi < textSentencesLength; tsi++)
                        {
                            // Check the time of one cycle, calculate the whole task run time, if it is more 10 sec, than percents will be shown - 1 state per second
                            Stopwatch stopWatch = new Stopwatch();
                            stopWatch.Start();

                            if (textSentencesLength < 20)
                            {
                                await Task.Delay(taskDelayTimeInSeconds * 1000); // delay to emulate upload of a real book - 
                            }

                            await _context.AddItemAsync(textSentences[tsi]);

                            stopWatch.Stop();
                            // Get the elapsed time as a TimeSpan value.
                            TimeSpan ts = stopWatch.Elapsed;
                            int tsMs = ts.Milliseconds;
                            uploadPercents.CurrentUploadingRecord = tsi;
                            uploadPercents.CurrentUploadedRecordRealTime = tsMs;
                            uploadPercents.TotalUploadedRealTime += tsMs;
                            bool resultSetKey1 = await SetTaskGuidKeys(uploadPercents, keysExistingTime);
                            //await _access.SetObjectAsync(uploadPercents.RedisKey, uploadPercents.FieldKeyPercents, uploadPercents, keysExistingTime);
                        };

                        // ключ guid создавать через хэш
                        bool removingResult = await RemoveKeysAfterRecording(desiredTextLanguage, guid);

                        // здесь проверить, что задание последнее и если да, то восстановить ключи данных
                        // to create sentenceCounts for current language
                        await _data.TotalRecordsCountWhereLanguageId(desiredTextLanguage);
                        //int totalLangSentences = await _data.FetchDataFromCache(desiredTextLanguage) ?? 0;

                        _logger.LogInformation("Queued Background Task RecordFileToDb {Guid} recorded " + textSentencesLength.ToString() + " records to DB", guid);
                    }
                    catch (Exception ex)
                    {
                        string message = "Queued Background Task RecordFileToDb {Guid} is crashed. \n {Message} \n";
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

        private async Task<bool> SetTaskGuidKeys(TaskUploadPercents uploadPercents, TimeSpan keysExistingTime)
        {
            await _access.SetObjectAsync(uploadPercents.RedisKey, uploadPercents.FieldKeyPercents, uploadPercents, keysExistingTime);            
            return true;
        }

        private async Task<bool> RemoveKeysAfterRecording(int languageId, string guid)
        {
            // to delete GetTotalCountWhereLanguageId:languageId
            bool removeKeyResult = await _data.RemoveTotalCountWhereLanguageId(languageId);

            // to remove redis keys about books
            string keyBase = "books";
            string keyHash = "data";
            var redisKey = $"{keyBase}:{keyHash}";
            bool removeKeyResult1 = await _access.RemoveAsync(redisKey);

            _logger.LogInformation("QBTask {Guid} has removed key {redisKey} successfully - {removeKeyResult1}", guid, redisKey, removeKeyResult1);

            return removeKeyResult && removeKeyResult1;
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
