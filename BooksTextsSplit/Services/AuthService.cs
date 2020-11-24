using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using CachingFramework.Redis;
using BooksTextsSplit.Models;
using BooksTextsSplit.Helpers;
using CachingFramework.Redis.Contracts.Providers;
using System;

namespace BooksTextsSplit.Services
{
    public interface IAuthService
    {
        Task<UserData> Authenticate(string email, string password);
        Task<UserData> AuthByToken(string authKey);
        Task Logout();
        //Task AuthenticateToCookie(string userName);
        //Task<IEnumerable<User>> GetAll();
    }
    public class AuthService : IAuthService
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly IAccessCacheData _access;
        private readonly ICosmosDbService _context;
        private readonly ICacheProviderAsync _cache;
        public AuthService(IHttpContextAccessor httpContext,
            ICosmosDbService cosmosDbService,
            IAccessCacheData access,
            ICacheProviderAsync cache)
        {
            _httpContext = httpContext;
            _access = access;
            _context = cosmosDbService;
            _cache = cache;
        }

        #region CreateUsers
        //Temporary - to create users in Redis only
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
        //        FirstName = "Alyona",
        //        LastName = "Gonchar",
        //        Username = "ALYGO",
        //        Token = "1234567890",
        //        Password = "ttt",
        //        Email = "alyona.gonchar@gmail.com" },
        //    new User {
        //        Id = 2,
        //        FirstName = "Kirill",
        //        LastName = "Gonchar",
        //        Username = "KIRGR",
        //        Token = "1234567890",
        //        Password = "ttt",
        //        Email = "gonchar.k.u@gmail.com" },
        //    new User {
        //        Id = 3,
        //        FirstName = "Alina",
        //        LastName = "Golitsina",
        //        Username = "ALIGO",
        //        Token = "1234567890",
        //        Password = "ttt",
        //        Email = "alina.golitsina@gmail.com" }
        //};
        // users hardcoded for simplicity, store in a db with hashed passwords in production applications        
        //private List<User> _users = new List<User>
        //{
        //    new User { 
        //        Id = 1, 
        //        FirstName = "Yuri", 
        //        LastName = "Gonchar", 
        //        Username = "YUGR",
        //        Token = "1234567890",
        //        Password = "ttt", 
        //        Email = "yuri.gonchar@gmail.com" }                
        //};
        #endregion

        public async Task<UserData> Authenticate(string email, string password)
        {
            #region CreateUsers
            // Temporary - to create users in Redis only
            //foreach (User u in _users)
            //{
            //    await _cache.SetObjectAsync(u.Email, u, System.TimeSpan.FromDays(10));
            //}
            #endregion

            bool isUsersExist = await _cache.KeyExistsAsync(email);

            if (!isUsersExist)
            {
                await SetUsersToCacheFromDb();
            }

            //var user = await Task.Run(() => _users.SingleOrDefault(x => x.Email == email && x.Password == password));
            UserData user = await _cache.GetObjectAsync<UserData>(email); // email == userKey for Redis

            if (user != null && user.Password == password)
            {
                await AuthByCookie(email);
                return user.WithoutPassword(); // authentication successful so return user details without password
            }
            return null; // return null if user not found or pswd is wrong
        }

        public async Task SetUsersToCacheFromDb()
        {
            List<UserData> allUsersData = await _context.GetUserListAsync<UserData>(0);
            foreach (UserData u in allUsersData)
            {
                string keyUser = u.Email;
                await _access.SetObjectAsync<UserData>(keyUser, u, TimeSpan.FromDays(1));
            }
            return;
        }

        public async Task Logout()
        {
            await _httpContext.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return;
        }

        #region LEGACY
        public async Task<UserData> AuthByToken(string fetchToken) // this.AuthenticationWithToken was changed on AuthenticationWithCoockie
        {
            UserData userWithToken = await _cache.GetObjectAsync<UserData>(fetchToken);
            if (userWithToken == null) // return null if user not found
            {
                return null;
            }
            if (userWithToken.Token == fetchToken)
            {
                // authentication successful so return user details without password
                return userWithToken.WithoutPassword();
            }
            return null;
        }
        #endregion

        private async Task AuthByCookie(string userName)
        {
            // создаем один claim
            var claims = new List<Claim> {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName)};

            // создаем объект ClaimsIdentity
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);

            // установка аутентификационных куки
            await _httpContext.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
            //await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
        }
    }
}

