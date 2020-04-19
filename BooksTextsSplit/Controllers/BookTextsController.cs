using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BooksTextsSplit.Models;

namespace BooksTextsSplit.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class BookTextsController : ControllerBase
    {
        private readonly BookContext _context;
        //private readonly INumberGen numberGen;

        public BookTextsController(BookContext context)
        {
            _context = context;
            //this.numberGen = numberGen;
        }

        // GET: api/Count/        
        [HttpGet("count")]
        public async Task<ActionResult<TotalCount>> GetTotalCount()
        {
            return new TotalCount(_context.BookTexts.Count());
            //return new TotalCount { sentencesCount = 5 };
        }

        // GET: api/Count/languageId
        // api/count/5?param=2
        [HttpGet("count/{languageId}")]
        public async Task<ActionResult<TotalCount>> GetTotalCount(int languageId, [FromQuery] int param)
        {
            return new TotalCount(_context.BookTexts.Where(i => i.LanguageId == languageId).Count());
            //return new TotalCount { sentencesCount = 5 };
        }

        // GET: api/BookTexts
        [HttpGet]
        public async Task<  ActionResult<     IEnumerable<TextSentence>      >> GetBookTexts()
        {
            return await _context.BookTexts.OrderBy(i => i.Id).ToListAsync();
        }

        // GET: api/BookTexts/BookText/languageId
        [HttpGet("BookText/{languageId}")]
        public async Task<ActionResult<BookText>> GetBookText(int languageId)
        {
            var bookText = new BookText { Sentences = _context.BookTexts.Where(i => i.LanguageId == languageId).ToList() };

            if (bookText == null)
            {
                return NotFound();
            }

            return bookText;
        }

        // PUT: api/BookTexts/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTodoItem(long id, TextSentence todoItem)
        {
            if (id != todoItem.Id)
            {
                return BadRequest();
            }

            _context.Entry(todoItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TodoItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/BookTexts
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<TextSentence>> PostTodoItem([FromBody]BookTextRequest textWrapper)
        {
            _context.BookTexts.AddRange(textWrapper.Text);
            await _context.SaveChangesAsync();

            // не знаю, нафиг это надо, мы в проекте везде возвращаем 
            // return Ok();
            //return CreatedAtAction("GetTodoItem", new { id = todoItem.Id }, todoItem);
            // return CreatedAtAction(nameof(GetTodoItem), new { ids = todoItems.Select(i => i.Id) }, todoItems);

            return Ok(new { ids = textWrapper.Text.Select(i => i.Id), totalCount = new TotalCount(_context.BookTexts.Where(i => i.LanguageId == textWrapper.LanguageId).Count()) });
        }

        // DELETE: api/BookTexts/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<TextSentence>> DeleteTodoItem(long id)
        {
            var todoItem = await _context.BookTexts.FindAsync(id);
            if (todoItem == null)
            {
                return NotFound();
            }

            _context.BookTexts.Remove(todoItem);
            await _context.SaveChangesAsync();

            return todoItem;
        }

        private bool TodoItemExists(long id)
        {
            return _context.BookTexts.Any(e => e.Id == id);
        }
    }
}
