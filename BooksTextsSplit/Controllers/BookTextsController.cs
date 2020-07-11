using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BooksTextsSplit.Models;
using BooksTextsSplit.Services;
using System.IO;
using Newtonsoft.Json;
using System.Reflection;
using System.ComponentModel.Design;

namespace BooksTextsSplit.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class BookTextsController : ControllerBase
    {
        private readonly ICosmosDbService _context;
        public BookTextsController(ICosmosDbService cosmosDbService)
        {
            _context = cosmosDbService;
        }

        // GET: api/Count/        
        [HttpGet("count")]
        public async Task<ActionResult<TotalCount>> GetTotalCount()
        {
            return new TotalCount((await _context.GetItemsAsync("SELECT * FROM c")).Count());
            //return new TotalCount { sentencesCount = 5 };
        }

        // GET: api/BookTexts/Count/languageId
        // api/BookTexts/count/5/?param=2
        [HttpGet("count/{languageId}")]
        public async Task<ActionResult<TotalCount>> GetTotalCount(int languageId, [FromQuery] int param)
        {
            return new TotalCount((await _context.GetItemsAsync("SELECT * FROM c")).Where(i => i.LanguageId == languageId).Count());
            //return new TotalCount { sentencesCount = 5 };
        }

        // GET: api/BookTexts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TextSentence>>> GetBookTexts()
        {
            return (await _context.GetItemsAsync("SELECT * FROM c")).OrderBy(i => i.Id).ToList();
        }

        // GET: api/BookTexts/BookUploadVersion/?bookId=1&languageId=0
        [HttpGet("BookUploadVersion")]
        public async Task<ActionResult<UploadedVersions>> GetBookUploadVersion([FromQuery] int bookId, [FromQuery] int languageId)
        {

            var versions = (await _context.GetItemsAsync($"SELECT * FROM c WHERE c.uploadVersion > 0 AND c.bookId = {bookId} AND c.languageId = {languageId} ORDER BY c.uploadVersion")).Select(s => s.UploadVersion).ToArray();

            int versionsLength = versions.Length;
            int maxUploadedVersion = 0;

            for (int i = 0; i < versionsLength; i++)
            {
                if (versions[i] > maxUploadedVersion)
                {
                    maxUploadedVersion = versions[i];
                }
            }

            var findVersions = new UploadedVersions(versions, maxUploadedVersion);
            return findVersions;
            //return new UploadedVersions(  ( await _context.GetItemsAsync("SELECT * FROM c")).Select(s => s.UploadVersion).ToArray()   );
        }

        // GET: api/BookTexts/BookText/languageId
        [HttpGet("BookText/{languageId}")]
        public async Task<ActionResult<BookText>> GetBookText(int languageId)
        {
            var bookText = new BookText { Sentences = (await _context.GetItemsAsync("SELECT * FROM c")).Where(i => i.LanguageId == languageId).ToList() };

            if (bookText == null)
            {
                return NotFound();
            }

            return bookText;
        }

        // GET: api/BookTexts/BooksIds/?where="bookSentenceId" & whereValue=1 & orderBy="bookId"
        [HttpGet("BooksIds")]
        public async Task<ActionResult<AllBooksIds>> GetBooksIds([FromQuery] string where, [FromQuery] int whereValue, [FromQuery] string orderBy)
        {
            bool areWhereOrderByRealProperties = true; //AreParamsRealTextSentenceProperties(where, orderBy);

            if (areWhereOrderByRealProperties)
            {
                var allFirstBookSentenceIdsSortedByBooksId = (await _context.GetItemsAsync($"SELECT * FROM c WHERE c.{where} = {whereValue} ORDER BY c.{orderBy}")).ToList(); // select the first sentences of all books and all books versions

                //int booksIdsLength = booksIds.Length;
                //int sortedBooksIdsLength = booksIds[booksIdsLength - 1];
                List<TextSentence> bookNamesSortedByIds = new List<TextSentence>();
                //TextSentence engBooksNames = new TextSentence();
                //TextSentence rusBooksNames = new TextSentence();
                //int[] sortedBooksIds = new int[sortedBooksIdsLength];
                int sortedBooksIdsIndex = 0;
                //int uploadBookVersion = 0;
                //int rusBooksNamesIndex = 0;
                //firstBookSentenceIds.Add(allFirstBookSentenceIdsSortedByBooksId[0]);
                int nextVersionOfBookId = 0;

                //for to find first uploadVersion only - new List
                //int versionOfBookId = allFirstBookSentenceIdsSortedByBooksId[i].UploadVersion;
                //int versionOfBookIdControl = allFirstBookSentenceIdsSortedByBooksId[i + 1].UploadVersion;
                //if (versionOfBookId != versionOfBookIdControl)
                //{
                //    return null;
                //}
                //else
                //{
                //    nextVersionOfBookId = versionOfBookId;
                //}

                int allFirstBookSentenceIdsSortedByBooksIdLength = allFirstBookSentenceIdsSortedByBooksId.Count();
                for (int i = 0; i < allFirstBookSentenceIdsSortedByBooksIdLength - 1; i += 2)
                {
                    var s0 = allFirstBookSentenceIdsSortedByBooksId[i];
                    var s1 = allFirstBookSentenceIdsSortedByBooksId[i + 1];
                    if (s0.LanguageId == 1 && s1.LanguageId == 0)// && s.UploadVersion == uploadBookVersion)
                    {
                        allFirstBookSentenceIdsSortedByBooksId[i] = s1;
                        allFirstBookSentenceIdsSortedByBooksId[i + 1] = s0;
                    }

                    //if (s.BookId > firstBookSentenceIds[sortedBooksIdsIndex].BookId && s.LanguageId == 1)
                    //{
                    //    sortedBooksIdsIndex++;
                    //    firstBookSentenceIds.Add(s);
                    //}
                    bookNamesSortedByIds.Add(allFirstBookSentenceIdsSortedByBooksId[i]);
                    sortedBooksIdsIndex++;

                }

                var findbooksIds = new AllBooksIds(bookNamesSortedByIds, sortedBooksIdsIndex);
                return findbooksIds;
            }
            else
            {
                return null;
            }
            //return new UploadedVersions(  ( await _context.GetItemsAsync("SELECT * FROM c")).Select(s => s.UploadVersion).ToArray()   );
        }

        public static bool IsPropertyOf<T>(string propertyName)
        {
          return typeof(T).GetProperties().Any(p => string.Equals(propertyName, p.Name, StringComparison.InvariantCultureIgnoreCase));
        }

        public static bool AreParamsRealPropertiesOf<T>(string[] testingProperties)
        {            
            int passedTestsCount = 0;            
            StringComparison comparison = StringComparison.InvariantCultureIgnoreCase; //Enum.GetValues(typeof(StringComparison)); - for StringComparison[]
            //bool exists = sample.GetType().GetProperties().Any(p => String.Equals(where, p.Name, comparison));
            foreach (var test in testingProperties)
            {
                foreach (var prop in typeof(T).GetProperties()) //sample.GetType()
                {
                    bool w = String.Equals(test, prop.Name, comparison);
                    if (w)
                    {                        
                        passedTestsCount++;
                    }                    
                }
            }
            return (passedTestsCount) == testingProperties.Length; //if all testingProperties are real it returns true
        }

        // POST: api/BookTexts
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<TextSentence>> PostBookText([FromBody] BookTextRequest textWrapper)
        {
            try
            {
                foreach (var s in textWrapper.Text)
                {
                    s.Id = Guid.NewGuid().ToString();
                    await _context.AddItemAsync(s);
                }
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }

            //_context.BookTexts.AddRange(textWrapper.Text);
            //await _context.SaveChangesAsync();

            // не знаю, нафиг это надо, мы в проекте везде возвращаем 
            // return Ok();
            //return CreatedAtAction("GetTodoItem", new { id = todoItem.Id }, todoItem);
            // return CreatedAtAction(nameof(GetTodoItem), new { ids = todoItems.Select(i => i.Id) }, todoItems);

            return Ok(new { ids = textWrapper.Text.Select(i => i.Id), totalCount = new TotalCount((await _context.GetItemsAsync("SELECT * FROM c")).Where(i => i.LanguageId == textWrapper.LanguageId).Count()) });
        }

        // POST: api/BookTexts/UploadFile
        [HttpPost("UploadFile")]
        // POST: api/BookTexts/UploadFile?language=0
        //public async Task<IActionResult> UploadFile([FromForm]IFormFile bookFile, [FromQuery] int language)
        public async Task<IActionResult> UploadFile([FromForm] IFormFile bookFile, [FromForm] int languageId, [FromForm] int bookId,
            [FromForm] int authorNameId, [FromForm] string authorName, [FromForm] int bookNameId, [FromForm] string bookName, [FromForm] int lastUploadedVersion)
        {
            if (bookFile != null)
            {
                string fileName = bookFile.FileName;
                StreamReader reader = new StreamReader(bookFile.OpenReadStream());
                string text = reader.ReadToEnd();

                IAllBookData _bookData = new AllBookData();
                ITextAnalysisLogicExtension analysisLogic = new TextAnalysisLogicExtension(_bookData);
                ISentencesDividingAnalysis sentenceAnalyser = new SentencesDividingAnalysis(_bookData, analysisLogic);
                //IAnalysisLogicParagraph paragraphAnalysis = new noneAnalysisLogicParagraph(bookData, msgService, analysisLogic);
                IChapterDividingAnalysis chapterAnalyser = new ChapterDividingAnalysis(_bookData, analysisLogic);
                IAllBookAnalysis bookAnalysis = new AllBookAnalysis(_bookData, analysisLogic, chapterAnalyser, sentenceAnalyser);

                int desiredTextLanguage = languageId;
                _bookData.SetFileToDo((int)WhatNeedDoWithFiles.AnalyseText, desiredTextLanguage);//создание нужной инструкции ToDo
                //bookData.SetFilePath(_filePath, desiredTextLanguage);
                string fileContent = text;
                _bookData.SetFileContent(fileContent, desiredTextLanguage);

                TextSentence[] textSentences = bookAnalysis.AnalyseTextBook();
                int textSentencesLength = textSentences.Length;
                string json = JsonConvert.SerializeObject(textSentences);
                int currentUploadingVersion = lastUploadedVersion + 1;

                try
                {
                    for (int tsi = 0; tsi < textSentencesLength; tsi++)
                    {
                        textSentences[tsi].Id = Guid.NewGuid().ToString();
                        textSentences[tsi].BookId = bookId;
                        textSentences[tsi].AuthorNameId = authorNameId;
                        textSentences[tsi].AuthorName = authorName;
                        textSentences[tsi].BookNameId = bookNameId;
                        textSentences[tsi].BookName = bookName;

                        textSentences[tsi].UploadVersion = currentUploadingVersion;

                        await _context.AddItemAsync(textSentences[tsi]);
                    }

                    //ParallelOptions p = new ParallelOptions {MaxDegreeOfParallelism = 2 };
                    //Parallel.ForEach(textSentences, p, async s => { await _context.AddItemAsync(s); });

                    //await Task.WhenAll(textSentences.Select(s => _context.AddItemAsync(s)));
                    //await Task.WhenAll(textSentences.Select(async s => await _context.AddItemAsync(s)));
                }
                catch (Exception ex)
                {
                    return Ok(ex.Message);
                }
                //return Ok(json);
                return Ok(textSentencesLength);
            }

            // var result = _bookAnalysis.Analyze(text);
            // _cosmosService.Save(result);

            return Problem("bad file");
        }

        // DELETE: api/BookTexts/a9da6acc-a5fa-4ed7-be90-4b0ec5d7c7cb/?bookId=1&languageId=0&uploadVersion=5
        [HttpDelete("{id}")]
        public async Task<ActionResult<IEnumerable<TextSentence>>> DeleteTextSentence(string id, [FromQuery] int bookId, [FromQuery] int languageId, [FromQuery] int uploadVersion)
        {
            try
            {
                var sentences = (await _context.GetItemsAsync("SELECT * FROM c")).Where(i => i.Id == id).ToArray();

                var sentence = sentences[0];

                if (sentence.BookId == bookId)
                {
                    if (sentence.LanguageId == languageId)
                    {
                        if (sentence.UploadVersion == uploadVersion)
                        {
                            await _context.DeleteItemAsync(id);
                        }
                    }
                }

                return Ok(sentence);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }
    }
}

