using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BooksTextsSplit.Models;
using BooksTextsSplit.Services;
using System.IO;
using Newtonsoft.Json;
using System.Reflection;
using System.ComponentModel.Design;
using System.Data;
using StackExchange.Redis;
using CachingFramework.Redis;
using Microsoft.Extensions.Localization;

namespace BooksTextsSplit.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]

    public class BookTextsController : ControllerBase
    {
        private readonly RedisContext cache;
        private readonly ICosmosDbService _context;
        // private readonly IDatabase _db;
        private IAuthService _authService;
        private readonly IStringLocalizer<BookTextsController> _localizer;

        public BookTextsController(
            ICosmosDbService cosmosDbService,
            RedisContext c,
            IAuthService authService,
            IStringLocalizer<BookTextsController> localizer) //, IDatabase db)
        {
            cache = c;
            _context = cosmosDbService;
            //_db = db;
            _authService = authService;
            _localizer = localizer;
        }

        //private List<User> _users = new List<User>
        //{
        //    new User {
        //        Id = 1,
        //        FirstName = "Yuri",
        //        LastName = "Gonchar",
        //        Username = "YUGR",
        //        Token = "1234567890",
        //        Password = "ttt",
        //        Email = "yuri.gonchar@gmail.com" },
        //    new User {
        //        Id = 2,
        //        FirstName = "222",
        //        LastName = "2222",
        //        Username = "22",
        //        Token = "1234567890",
        //        Password = "ttt",
        //        Email = "222.2222@gmail.com" },
        //    new User {
        //        Id = 3,
        //        FirstName = "333",
        //        LastName = "3333",
        //        Username = "33",
        //        Token = "1234567890",
        //        Password = "ttt",
        //        Email = "333.3333@gmail.com" }
        //};

        #region GET

        // GET: api/BookTexts/loc/ - for localizer testing
        [AllowAnonymous]
        [HttpGet("loc")]
        public async Task<ActionResult<string>> GetLoc()
        {
            return Ok(_localizer["ResultCode0"]);
        }

        // GET: api/BookTexts/auth/getMe/
        //[Authorize] + fetch from middleware BasicAuthenticationHandler user of token (context)
        [HttpGet("auth/getMe")]
        public ActionResult<LoginAttemptResult> GetMe([FromServices] User context) //why I cannot use IActionResult ?
        {
            LoginAttemptResult resultData = new LoginAttemptResult
            {
                AuthUser = new User
                {
                    Id = context.Id,
                    FirstName = context.FirstName,
                    LastName = context.LastName,
                    Username = context.Username,
                    Email = context.Email
                },
                ResultMessage="",
                ResultCode = 0
            };
            return resultData;
            //return Ok(users);
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
        [HttpPost("auth/login")]

        public async Task<ActionResult<LoginAttemptResult>> UploadLoginData([FromBody] LoginDataFromUI fetchedLoginData) //async Task<IActionResult>
        {
            User user = await _authService.Authenticate(fetchedLoginData.Email, fetchedLoginData.Password);
            if (user.Email == fetchedLoginData.Email)
            {
                string newToken = await CreateToken(fetchedLoginData.Email);
                if (newToken == null)
                {
                    return ResultData(1, null);
                }
                return ResultData(0, newToken);
            }
            return ResultData(1, null);
            //return Ok(user);
        }

        public LoginAttemptResult ResultData(int resultCode, string newToken)
        {
            LoginAttemptResult resultData = new LoginAttemptResult();
            if (resultCode == 0)
            {
                resultData.IssuedToken = newToken;
            }
            resultData.ResultMessage = _localizer["ResultCode" + resultCode];
            resultData.ResultCode = resultCode;
            return resultData;
        }

        public async Task<string> CreateToken(string emailKey)
        {
            User user = await cache.Cache.GetObjectAsync<User>(emailKey);
            user.Token = "4db6A12C94kfv51qaxB2sdgf781xvf11dfnhsr3382gui914asc6A12C94acdfb51cbB2avs781db1";
            // Set Users to Redis
            //foreach(User u in _users)
            //{
            //    await cache.Cache.SetObjectAsync(u.Email, u, TimeSpan.FromDays(1));
            //}            

            // Set Token to Redis                                          
            await cache.Cache.SetObjectAsync(user.Token, user, TimeSpan.FromDays(1));
            User getNewToken = await cache.Cache.GetObjectAsync<User>(user.Token);
            if (getNewToken.Token == user.Token)
            {
                return getNewToken.Token;
            }
            else
            {
                return null;
            }
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

        #endregion

        #region DELETE

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

