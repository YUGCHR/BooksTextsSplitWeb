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
            return new TotalCount( (await _context.GetItemsAsync("SELECT * FROM c")).Count() );
            //return new TotalCount { sentencesCount = 5 };
        }

        // GET: api/Count/languageId
        // api/count/5?param=2
        [HttpGet("count/{languageId}")]
        public async Task<ActionResult<TotalCount>> GetTotalCount(int languageId, [FromQuery] int param)
        {
            return new TotalCount((await _context.GetItemsAsync("SELECT * FROM c")).Where(i => i.LanguageId == languageId).Count());
            //return new TotalCount { sentencesCount = 5 };
        }

        // GET: api/BookTexts
        [HttpGet]
        public async Task<  ActionResult<     IEnumerable<TextSentence>      >> GetBookTexts()
        {
            return (await _context.GetItemsAsync("SELECT * FROM c")).OrderBy(i => i.Id).ToList();
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

        [HttpPost("UploadFile")]
        public async Task<IActionResult> UploadFile([FromForm]IFormFile bookFile)
        {
            if (bookFile != null)
            {
                StreamReader reader = new StreamReader(bookFile.OpenReadStream());
                string text = reader.ReadToEnd();

                IAllBookData _bookData = new AllBookData();                
                ITextAnalysisLogicExtension analysisLogic = new TextAnalysisLogicExtension(_bookData);
                ISentencesDividingAnalysis sentenceAnalyser = new SentencesDividingAnalysis(_bookData, analysisLogic);
                //IAnalysisLogicParagraph paragraphAnalysis = new noneAnalysisLogicParagraph(bookData, msgService, analysisLogic);
                IChapterDividingAnalysis chapterAnalyser = new ChapterDividingAnalysis(_bookData, analysisLogic);
                IAllBookAnalysis bookAnalysis = new AllBookAnalysis(_bookData, analysisLogic, chapterAnalyser, sentenceAnalyser);

                int desiredTextLanguage = 0;
                _bookData.SetFileToDo((int)WhatNeedDoWithFiles.AnalyseText, desiredTextLanguage);//создание нужной инструкции ToDo
                //bookData.SetFilePath(_filePath, desiredTextLanguage);
                string fileContent = text;
                _bookData.SetFileContent(fileContent, desiredTextLanguage);

                string hash = bookAnalysis.AnalyseTextBook();
                
                return Ok(hash);
            }

            //
            // var result = _bookAnalysis.Analyze(text);
            // _cosmosService.Save(result);

            return Problem("bad file");
        }

                // DELETE: api/BookTexts/5
                //[HttpDelete("{id}")]
                //public async Task<ActionResult<TextSentence>> DeleteTodoItem(long id)
                //{
                //    var todoItem = await _context.BookTexts.FindAsync(id);
                //    if (todoItem == null)
                //    {
                //        return NotFound();
                //    }

                //    _context.BookTexts.Remove(todoItem);
                //    await _context.SaveChangesAsync();

                //    return todoItem;
                //}

                //private bool TodoItemExists(long id)
                //{
                //    return _context.BookTexts.Any(e => e.Id == id);
                //}
            }
}
