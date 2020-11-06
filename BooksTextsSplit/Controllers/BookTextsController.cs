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
using CachingFramework.Redis.Contracts.Providers;

namespace BooksTextsSplit.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]

    public class BookTextsController : ControllerBase
    {
        #region Declarations
        private IBackgroungTasksService _task2Queue;
        private readonly ILogger<BookTextsController> _logger;
        private readonly IControllerDataManager _data;
        private readonly ICacheProviderAsync _cache;
        private readonly IAccessCacheData _access;
        private readonly ICosmosDbService _context;
        private IAuthService _authService;
        private IResultDataService _result;

        public BookTextsController(
            IBackgroungTasksService task2Queue,
            ILogger<BookTextsController> logger,
            IControllerDataManager data,
            ICosmosDbService cosmosDbService,
            ICacheProviderAsync cache,
            IAccessCacheData access,
            IAuthService authService,
            IResultDataService resultDataService)
        {
            _task2Queue = task2Queue;
            _logger = logger;
            _data = data;
            _cache = cache;
            _access = access;
            _context = cosmosDbService;
            _authService = authService;
            _result = resultDataService;
        }
        #endregion

        #region Auth

        // GET: api/BookTexts/auth/init/ - check is user logged in on app start
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
            // TODO - Get all user data from Redis and return it to UI
            if (User?.Identity == null)
            {
                return await _result.ResultData(2, null);
            }
            return await _result.ResultData(0, userEmail);
        }

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
        #endregion

        #region Upload
        // GET: api/BookTexts/worker/
        [HttpGet("worker")]
        public ActionResult GetWorker()
        {
            string guid = Guid.NewGuid().ToString();
            _task2Queue.WorkerSample(guid);

            return Ok("Queued Background Task is starting");
        }

        // GET: api/BookTexts/uploadTaskPercents/?taskGuid = e0ff4648-b183-49c7-b3d9-bc9fc99dcf8e
        [HttpGet("uploadTaskPercents")]
        public async Task<ActionResult<TaskUploadPercents>> GetUploadTaskPercents([FromQuery] string taskGuid)
        {
            int percentDecrement = 0;
            var taskStateCurrent = await _access.GetObjectAsync<TaskUploadPercents>(taskGuid);
            int percentCurrent = taskStateCurrent.DoneInPercents;
            int percentChanged = percentCurrent;

            while ((percentCurrent <= percentChanged + percentDecrement) && (percentCurrent < 99))
            {
                percentChanged = percentCurrent;
                await Task.Delay(100);
                taskStateCurrent = await _access.GetObjectAsync<TaskUploadPercents>(taskGuid);
                percentCurrent = taskStateCurrent.DoneInPercents;
            }
            return taskStateCurrent;
        }

        // POST: api/BookTexts/UploadFile        
        [HttpPost("UploadFile")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile bookFile, [FromForm] string jsonBookDescription)
        {
            // it's need to check the Redis keys after new books were recorded to Db
            if (bookFile != null)
            {
                string guid = Guid.NewGuid().ToString();
                TaskUploadPercents uploadPercents = new TaskUploadPercents
                {
                    CurrentTaskGuid = guid,
                    CurrentUploadingBookId = 0, // may be put whole TextSentence?
                    RecordrsTotalCount = 0,
                    CurrentUploadingRecord = 0,
                    DoneInPercents = 0,
                };

                //Redis key initialization - must be not null for GetUploadTaskPercents
                await _cache.SetObjectAsync(guid, uploadPercents, TimeSpan.FromDays(1));

                _task2Queue.RecordFileToDbInBackground(bookFile, jsonBookDescription, guid);

                return Ok(guid);
            }
            return Problem("bad file");
        }
        #endregion

        #region GetCounts

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
            //return new TotalCount((await _context.GetItemsAsync($"SELECT * FROM c WHERE c.{dbWhere} = {languageId}")).Count());
            int totalLangSentences = await _data.FetchDataFromCache(languageId) ?? 0;
            return new TotalCount(totalLangSentences);
        }
        // System.NotSupportedException: Deserialization of reference types without parameterless constructor is not supported. Type 'BooksTextsSplit.Models.TotalCount'
        // System.Text.Json.JsonException: The JSON value could not be converted to System.Int32. Path: $ | LineNumber: 0 | BytePositionInLine: 1.
        #endregion

        #region GetBooks for SelectPage

        // GET: api/BookTexts/BooksNamesIds/?where="bookId"&whereValue=1&startUploadVersion=1 - fetching list of all BookIds existing in Db
        [HttpGet("BooksNamesIds")]
        public async Task<ActionResult<BooksNamesExistInDb>> GetBooksNamesIds([FromQuery] string where, [FromQuery] int whereValue, [FromQuery] int startUploadVersion)
        {
            return await _data.FetchBooksNamesIds(where, whereValue, startUploadVersion);
        }

        // GET: api/BookTexts/BookNameVersions/?where="bookId"&whereValue=1&bookId=(from selection) - fetching list of all uploaded versions for selected BookIds
        [HttpGet("BookNameVersions")]
        public async Task<ActionResult<BooksVersionsExistInDb>> GetBookNameVersions([FromQuery] string where, [FromQuery] int whereValue, [FromQuery] int bookId)
        {
            return await _data.FetchBookNameVersions(where, whereValue, bookId);
        }

        // GET: api/BookTexts/BooksPairTexts/?where1=bookId&where1Value=(selected)&where2=uploadVersion&where2Value=(selected) - fetching selected version of the selected books pair texts
        [HttpGet("BooksPairTexts")]
        public async Task<ActionResult<BooksPairTextsFromDb>> GetBooksPairTexts([FromQuery] string where1, [FromQuery] int where1Value, [FromQuery] string where2, [FromQuery] int where2Value)
        {
            return await _data.FetchBooksPairTexts(where1, where1Value, where2, where2Value);
        }
        #endregion

        #region LEGACY!

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

