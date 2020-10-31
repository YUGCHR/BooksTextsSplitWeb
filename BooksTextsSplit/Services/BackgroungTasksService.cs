using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using BooksTextsSplit.Models;
using System.IO;
using System.Text.Json;
using CachingFramework.Redis;
using Microsoft.Extensions.Logging;

namespace BooksTextsSplit.Services
{
    public interface IBackgroungTasksService
    {
        void RecordFileToDbInBackground(IFormFile bookFile, string jsonBookDescription, string guid);
        void WorkerSample();
    }
    public class BackgroungTasksService : IBackgroungTasksService
    {
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly ILogger<BackgroungTasksService> _logger;
        private readonly RedisContext cache;
        private readonly ICosmosDbService _context;

        public BackgroungTasksService(
            IBackgroundTaskQueue taskQueue,
            ILogger<BackgroungTasksService> logger,            
            ICosmosDbService cosmosDbService,
            RedisContext c)
        {
            _taskQueue = taskQueue;
            _logger = logger;            
            cache = c;
            _context = cosmosDbService;            
        }

        public void RecordFileToDbInBackground(IFormFile bookFile, string jsonBookDescription, string guid)
        {
            JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, };
            TextSentence bookDescription = JsonSerializer.Deserialize<TextSentence>(jsonBookDescription, options);
            string fileName = bookFile.FileName;
            StreamReader reader = new StreamReader(bookFile.OpenReadStream());
            string text = reader.ReadToEnd();

            IAllBookData _bookData = new AllBookData();
            ITextAnalysisLogicExtension analysisLogic = new TextAnalysisLogicExtension(_bookData);
            ISentencesDividingAnalysis sentenceAnalyser = new SentencesDividingAnalysis(_bookData, analysisLogic);
            //IAnalysisLogicParagraph paragraphAnalysis = new noneAnalysisLogicParagraph(bookData, msgService, analysisLogic);
            IChapterDividingAnalysis chapterAnalyser = new ChapterDividingAnalysis(_bookData, analysisLogic);
            IAllBookAnalysis bookAnalysis = new AllBookAnalysis(_bookData, analysisLogic, chapterAnalyser, sentenceAnalyser);

            int desiredTextLanguage = bookDescription.LanguageId;
            //создание нужной инструкции ToDo
            _bookData.SetFileToDo((int)WhatNeedDoWithFiles.AnalyseText, desiredTextLanguage);
            //bookData.SetFilePath(_filePath, desiredTextLanguage);

            string fileContent = text;
            _bookData.SetFileContent(fileContent, desiredTextLanguage);
            // Enqueue a background work item
            _taskQueue.QueueBackgroundWorkItem(async token =>
            {
                try
                {
                    _logger.LogInformation(
                    "Queued Background Task RecordFileToDb {Guid} is starting", guid);

                    // TODO it's necessary to pass bookDescription to AnalyseTextBook and use bookDescription.properties when initielize textSentences[]
                    TextSentence[] textSentences = bookAnalysis.AnalyseTextBook();
                    int textSentencesLength = textSentences.Length;
                    string json = JsonSerializer.Serialize(textSentences);
                    int currentUploadingVersion = bookDescription.UploadVersion + 1;

                    try
                    {
                        _logger.LogInformation(
                            "Queued Background Task RecordFileToDb {Guid} is running", guid);

                        for (int tsi = 0; tsi < textSentencesLength; tsi++)
                        {
                            textSentences[tsi].Id = Guid.NewGuid().ToString();
                            textSentences[tsi].BookId = bookDescription.BookId;
                            textSentences[tsi].AuthorNameId = bookDescription.AuthorNameId;
                            textSentences[tsi].AuthorName = bookDescription.AuthorName;
                            textSentences[tsi].BookNameId = bookDescription.BookNameId;
                            textSentences[tsi].BookName = bookDescription.BookName;

                            textSentences[tsi].UploadVersion = currentUploadingVersion;
                            // TODO add the key with percents to Redis
                            await _context.AddItemAsync(textSentences[tsi]);
                        };

                        _logger.LogInformation(
                            "Queued Background Task RecordFileToDb {Guid} recorded "
                            + textSentencesLength.ToString()
                            + " records to DB", guid);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation(
                            "Queued Background Task RecordFileToDb {Guid} is crashed. " + ex.Message, guid);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Prevent throwing if the Delay is cancelled
                }
            });
            //return "Task " + guid + " was Queued Background";
        }

        public void WorkerSample()
        {
            // Enqueue a background work item
            _taskQueue.QueueBackgroundWorkItem(async token =>
            {
                // Simulate three 5-second tasks to complete
                // for each enqueued work item

                int delayLoop = 0;
                string guid = Guid.NewGuid().ToString();

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
