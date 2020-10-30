using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BooksTextsSplit.Models;
using BooksTextsSplit.Services;
using System.IO;
using System.Text.Json;
using System.Reflection;
using System.ComponentModel.Design;
using System.Data;
using StackExchange.Redis;
using CachingFramework.Redis;
using Microsoft.Extensions.Localization;
using BooksTextsSplit.Helpers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BooksTextsSplit.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]

    public class BookTextsController : ControllerBase
    {
        private IBackgroungTasksService _task2Queue;
        private readonly ILogger<BookTextsController> _logger;
        //private readonly CancellationToken _cancellationToken;
        private readonly RedisContext cache;
        private readonly ICosmosDbService _context;
        // private readonly IDatabase _db;
        private IAuthService _authService;
        private IResultDataService _result;

        public BookTextsController(
            IBackgroungTasksService task2Queue,
            ILogger<BookTextsController> logger,
            //IHostApplicationLifetime applicationLifetime,
            ICosmosDbService cosmosDbService,
            RedisContext c,
            IAuthService authService,
            IResultDataService resultDataService) //, IDatabase db)
        {
            _task2Queue = task2Queue;
            _logger = logger;
            //_cancellationToken = applicationLifetime.ApplicationStopping;
            cache = c;
            _context = cosmosDbService;
            //_db = db;
            _authService = authService;
            _result = resultDataService;
        }

        #region GET

        // GET: api/BookTexts/auth/init/ - for localizer testing
        [AllowAnonymous]
        [HttpGet("auth/init")]
        public ActionResult<string> GetInit()
        {
            string userEmail = User?.Identity?.Name;
            var isLoggedIn = (userEmail != null);
            return Ok(new { IsLoggedIn = isLoggedIn }); //(_localizer["ResultCode0"]);
        }

        // GET: api/BookTexts/auth/getMe/
        //[Authorize] + fetch from middleware BasicAuthenticationHandler user of token (context)
        [HttpGet("auth/getMe")]
        //[ValidateAntiForgeryToken]
        //public ActionResult<LoginAttemptResult> GetMe([FromServices] User context) // - context from BasicAuthenticationHandler
        public async Task<ActionResult<LoginAttemptResult>> GetMe()
        {
            string userEmail = User?.Identity?.Name;
            if (User?.Identity == null)
            {
                return await _result.ResultData(2, null);
            }
            return await _result.ResultData(0, userEmail);
        }

        // GET: api/BookTexts/worker/
        [HttpGet("worker")]
        public ActionResult GetWorker()
        {
            _task2Queue.WorkerSample();

            return Ok("Queued Background Task is starting");
        }

        // GET: api/BookTexts/Count/        
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

        //------------------------------------------------------------------------------------

        // GET: api/BookTexts/BooksNamesIds/?where="bookId"&whereValue=1&startUploadVersion=1 - fetching list of all BookIds existing in Db
        [HttpGet("BooksNamesIds")]
        public async Task<ActionResult<BooksNamesExistInDb>> GetBooksNamesIds([FromQuery] string where, [FromQuery] int whereValue, [FromQuery] int startUploadVersion)
        {
            return await FetchBooksNamesIds(where, whereValue, startUploadVersion);
        }

        public async Task<BooksNamesExistInDb> FetchBooksNamesIds(string where, int whereValue, int startUploadVersion)
        {
            string bookSentenceIdKey = where + ":" + whereValue.ToString();
            List<TextSentence> requestedSelectResult = await cache.Cache.FetchObjectAsync<List<TextSentence>>(bookSentenceIdKey, () => FetchBooksNamesFromDb(where, whereValue));

            List<TextSentence> toSelectBookNameFromAll = requestedSelectResult.Where(r => r.UploadVersion == startUploadVersion).ToList();

            IEnumerable<IGrouping<int, TextSentence>> allBooksNamesPairings = toSelectBookNameFromAll.GroupBy(r => r.BookId);
            BooksNamesExistInDb foundBooksIds = new BooksNamesExistInDb
            {
                BookNamesVersion1SortedByIds = allBooksNamesPairings.Select(p => new BooksNamesSortByLanguageIdSortByBookId
                {
                    BookId = p.Key,
                    BooksDescriptions = p.OrderBy(s => s.LanguageId).Select(s => new BooksNamesSortByLanguageId { LanguageId = s.LanguageId, Sentence = s }).ToList()
                }
                ).ToList()
            };

            return foundBooksIds;
        }

        // GET: api/BookTexts/BookNameVersions/?where="bookId"&whereValue=1&bookId=(from selection) - fetching list of all uploaded versions for selected BookIds
        [HttpGet("BookNameVersions")]
        public async Task<ActionResult<BooksVersionsExistInDb>> GetBookNameVersions([FromQuery] string where, [FromQuery] int whereValue, [FromQuery] int bookId)
        {
            return await FetchBookNameVersions(where, whereValue, bookId);
        }

        public async Task<BooksVersionsExistInDb> FetchBookNameVersions(string where, int whereValue, int bookId)
        {
            string bookSentenceIdKey = where + ":" + whereValue.ToString();
            List<TextSentence> requestedSelectResult = await cache.Cache.FetchObjectAsync<List<TextSentence>>(bookSentenceIdKey, () => FetchBooksNamesFromDb(where, whereValue));

            IEnumerable<IGrouping<int, TextSentence>> languageIdGrouping = requestedSelectResult.Where(r => r.BookId == bookId).ToList().GroupBy(r => r.LanguageId);

            BooksVersionsExistInDb foundBooksVersion = new BooksVersionsExistInDb
            {
                SelectedBookIdAllVersions = languageIdGrouping.Select(p => new SelectedBookIdGroupByLanguageId
                {
                    LanguageId = p.Key,
                    Sentences = p.OrderBy(v => v.UploadVersion).Select(s => s).ToList()
                }
                ).OrderBy(s => s.LanguageId).ToList()
            };
            #region for_memory_grouping_in_grouping
            // на память - группировка по LanguageId внутри группировки по BookId
            IEnumerable<IGrouping<int, TextSentence>> allVersionsPairings = requestedSelectResult.GroupBy(r => r.BookId);
            BooksVersionsExistInDb_Memory foundBooksVersion_Memory = new BooksVersionsExistInDb_Memory
            {
                AllVersionsOfBooksNames = allVersionsPairings.Select(p => new BooksVersionsGroupedByBookIdGroupByLanguageId
                {
                    BookId = p.Key,
                    BookVersionsDescriptions = p.GroupBy(l => l.LanguageId).Select(g => new BooksVersionsGroupByLanguageId_Memory
                    {
                        LanguageId = g.Key,
                        Sentences = g.OrderBy(v => v.UploadVersion).Select(t => t).ToList()
                    }
                    ).OrderBy(s => s.LanguageId).ToList()
                }
                ).ToList()
            };
            #endregion
            return foundBooksVersion;
        }

        public async Task<List<TextSentence>> FetchBooksNamesFromDb(string where, int whereValue)
        {
            // bool areWhereOrderByRealProperties = true; //AreParamsRealTextSentenceProperties(where, orderBy); - it is needs to add checking of parameters existing 

            List<TextSentence> requestedSelectResult = (await _context.GetItemsAsync
                ($"SELECT * FROM c WHERE c.{where} = {whereValue}"))
                .OrderBy(li => li.BookId) // if it remove the sort, the both methods will be the same
                .ThenBy(uv => uv.UploadVersion)
                .ThenBy(bi => bi.LanguageId)
                .ToList();

            // Set List to Redis
            string createdKeyNameFromRequest = where + ":" + whereValue.ToString(); //выдачу из базы сохранить как есть, с ключом bookSentenceId:1                                                                                
            await cache.Cache.SetObjectAsync(createdKeyNameFromRequest, requestedSelectResult, TimeSpan.FromDays(1));

            return requestedSelectResult;
        }

        // GET: api/BookTexts/BooksPairTexts/?where1=bookId&where1Value=(selected)&where2=uploadVersion&where2Value=(selected) - fetching selected version of the selected books pair texts
        [HttpGet("BooksPairTexts")]
        public async Task<ActionResult<BooksPairTextsFromDb>> GetBooksPairTexts([FromQuery] string where1, [FromQuery] int where1Value, [FromQuery] string where2, [FromQuery] int where2Value)
        {
            return await FetchBooksPairTexts(where1, where1Value, where2, where2Value);
        }

        public async Task<BooksPairTextsFromDb> FetchBooksPairTexts(string where1, int where1Value, string where2, int where2Value)
        {
            string booksPairTextsKey = where1 + ":" + where1Value.ToString();
            List<TextSentence> requestedSelectResult = await cache.Cache.FetchObjectAsync<List<TextSentence>>(booksPairTextsKey, () => FetchBooksTextsFromDb(where1, where1Value));

            //where2 must be uploadVersion for the next grouping
            //TODO get UploadVersion from where2
            IEnumerable<IGrouping<int, TextSentence>> languageIdGrouping = requestedSelectResult.Where(r => r.UploadVersion == where2Value).ToList().GroupBy(r => r.LanguageId);

            BooksPairTextsFromDb foundBooksPairTexts = new BooksPairTextsFromDb // selectedBooksPairTexts
            {
                SelectedBooksPairTexts = languageIdGrouping.Select(p => new BooksPairTextsGroupByLanguageId
                {
                    LanguageId = p.Key,
                    Sentences = p.OrderBy(v => v.BookSentenceId).Select(s => s).ToList()
                }
                ).OrderBy(s => s.LanguageId).ToList()
            };

            return foundBooksPairTexts;
        }

        public async Task<List<TextSentence>> FetchBooksTextsFromDb(string where, int whereValue)
        {
            // bool areWhereOrderByRealProperties = true; //AreParamsRealTextSentenceProperties(where, orderBy); - it is needs to add checking of parameters existing 

            List<TextSentence> requestedSelectResult = (await _context.GetItemsAsync
                ($"SELECT * FROM c WHERE c.{where} = {whereValue}"))
                .OrderBy(uv => uv.UploadVersion)
                .ThenBy(bi => bi.LanguageId)
                .ThenBy(si => si.BookSentenceId)
                .ToList();

            // Set List to Redis
            string createdKeyNameFromRequest = where + ":" + whereValue.ToString(); //выдачу из базы сохранить как есть, с ключом bookId:(selected BookId)                                                                                
            await cache.Cache.SetObjectAsync(createdKeyNameFromRequest, requestedSelectResult, TimeSpan.FromDays(1));

            return requestedSelectResult;
        }

        //------------------------------------------------------------------------------------



        // LEGACY !
        // SAMPLE with AreParamsRealTextSentenceProperties check
        // GET: api/BookTexts/BooksIds/?where="bookSentenceId"&whereValue=1&orderBy="bookId"&needPostSelect=true&postWhere="UploadVersion"&postWhereValue=1
        [HttpGet("BooksIds")]
        public async Task<ActionResult<BooksNamesExistInDb>> GetBooksIds([FromQuery] string where, [FromQuery] int whereValue, [FromQuery] string orderBy, [FromQuery] bool needPostSelect, [FromQuery] string postWhere, [FromQuery] int postWhereValue)
        {
            //db.StringSet(BitConverter.GetBytes(5), "asdf");

            bool areWhereOrderByRealProperties = true; //AreParamsRealTextSentenceProperties(where, orderBy);

            if (areWhereOrderByRealProperties)
            {
                List<TextSentence> requestedSelectResult = (await _context.GetItemsAsync($"SELECT * FROM c WHERE c.{where} = {whereValue} ORDER BY c.{orderBy}")).ToList(); // select the first sentences of all books and all books versions

                needPostSelect = true;
                postWhere = "UploadVersion";
                postWhereValue = 1; // UploadVersion == 0 will be for seperate method (to set the book header) - may be
                List<TextSentence> requestedSelectResultSorted = new List<TextSentence>();
                int sortedBooksIdsCount = 0;
                if (needPostSelect)
                {
                    foreach (var r in requestedSelectResult)
                    {
                        if (r.UploadVersion == postWhereValue) //it needs to get property name from postWhere
                        {
                            requestedSelectResultSorted.Add(r);
                            sortedBooksIdsCount++;
                        }
                    }
                }

                IEnumerable<IGrouping<int, TextSentence>> pairings = requestedSelectResultSorted.GroupBy(r => r.BookId);

                BooksNamesExistInDb foundBooksIds = new BooksNamesExistInDb
                {
                    BookNamesVersion1SortedByIds = pairings.Select(p => new BooksNamesSortByLanguageIdSortByBookId
                    {
                        BookId = p.Key,
                        BooksDescriptions = p.OrderBy(s => s.LanguageId).Select(s => new BooksNamesSortByLanguageId { LanguageId = s.LanguageId, Sentence = s }).ToList()
                    }
                    ).ToList()
                };

                return foundBooksIds;
            }
            else
            {
                return null;
            }
            //return new UploadedVersions(  ( await _context.GetItemsAsync("SELECT * FROM c")).Select(s => s.UploadVersion).ToArray()   );
        }

        public static object GetProperty(object obj, string propertyName)
        {
            PropertyInfo pinfo = obj.GetType().GetProperty(propertyName);

            // note the null propagation ?.
            return pinfo?.GetValue(obj);
        }

        public static object GetPropertyGeneric<T>(T obj, string propertyName)
        {
            PropertyInfo pinfo = typeof(T).GetProperty(propertyName);

            // note the null propagation ?.
            return pinfo?.GetValue(obj);
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

        #endregion

        #region POST

        // POST: api/BookTexts/auth/login/
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        [HttpPost("auth/login")]

        public async Task<ActionResult<LoginAttemptResult>> Login([FromBody] LoginDataFromUI fetchedLoginData) //async Task<IActionResult>
        {
            User user = await _authService.Authenticate(fetchedLoginData.Email, fetchedLoginData.Password);
            if (user == null)
            {
                return await _result.ResultDataWithToken(3, null);
            }
            return await _result.ResultDataWithToken(0, user);
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

            // _context.BookTexts.AddRange(textWrapper.Text);
            // await _context.SaveChangesAsync();

            // не знаю, нафиг это надо, мы в проекте везде возвращаем 
            // return Ok();
            // return CreatedAtAction("GetTodoItem", new { id = todoItem.Id }, todoItem);
            // return CreatedAtAction(nameof(GetTodoItem), new { ids = todoItems.Select(i => i.Id) }, todoItems);

            return Ok(new
            {
                ids = textWrapper.Text.Select(i => i.Id),
                totalCount = new TotalCount((await _context.GetItemsAsync("SELECT * FROM c"))
                    .Where(i => i.LanguageId == textWrapper.LanguageId)
                    .Count())
            });
        }





        // POST: api/BookTexts/UploadFile        
        [HttpPost("UploadFile")]        
        public IActionResult UploadFile([FromForm] IFormFile bookFile, [FromForm] string jsonBookDescription)
        {
            // it's need to check the Redis keys after new books were recorded to Db
            if (bookFile != null)
            {
                _task2Queue.RecordFileToDbInBackground(bookFile, jsonBookDescription);
                return Ok("Task was Queued Background");
            }
            return Problem("bad file");
        }

        





        #endregion

        #region DELETE

        // DELETE: api/BookTexts/logout
        [HttpDelete("auth/logout")]
        public async Task<ActionResult> DeleteLogout()
        {
            await _authService.Logout();
            return Ok("exit");
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

        #endregion

    }
}

