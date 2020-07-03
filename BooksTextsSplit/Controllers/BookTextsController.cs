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
            return new TotalCount((await _context.GetItemsAsync("SELECT * FROM c")).Count() );
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
        
        // POST: api/BookTexts
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<TextSentence>> PostBookText([FromBody]BookTextRequest textWrapper)
        {
            try
            {            
            foreach (var s in textWrapper.Text) 
            {
                s.Id = Guid.NewGuid().ToString();
                await _context.AddItemAsync(s);
            }
            }
            catch(Exception ex)
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
        public async Task<IActionResult> UploadFile([FromForm]IFormFile bookFile, [FromForm] int languageId, [FromForm] int bookId, 
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

                        //await _context.AddItemAsync(textSentences[tsi]);
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
        public async Task <ActionResult <IEnumerable <TextSentence>>> DeleteTextSentence(string id, [FromQuery] int bookId, [FromQuery] int languageId, [FromQuery] int uploadVersion)
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

