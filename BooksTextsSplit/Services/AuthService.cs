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

namespace BooksTextsSplit.Services
{
    public interface IAuthService
    {
        Task<User> Authenticate(string email, string password);
        Task<User> AuthByToken(string authKey);
        Task Logout();
        //Task AuthenticateToCookie(string userName);
        //Task<IEnumerable<User>> GetAll();
    }
    public class AuthService : IAuthService
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly ICacheProviderAsync _cache;
        public AuthService(IHttpContextAccessor httpContext, ICacheProviderAsync cache)
        {
            _httpContext = httpContext;
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

        public async Task<User> Authenticate(string email, string password)
        {
            #region CreateUsers
            // Temporary - to create users in Redis only
            //foreach (User u in _users)
            //{
            //    await cache.Cache.SetObjectAsync(u.Email, u, System.TimeSpan.FromDays(10));
            //}
            #endregion

            //var user = await Task.Run(() => _users.SingleOrDefault(x => x.Email == email && x.Password == password));
            User user = await _cache.GetObjectAsync<User>(email); // email == userKey for Redis
            
            if (user != null && user.Password == password)
            {                
                await AuthByCookie(email);
                return user.WithoutPassword(); // authentication successful so return user details without password
            }            
            return null; // return null if user not found or pswd is wrong
        }

        public async Task Logout()
        {
            await _httpContext.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return;
        }

        #region LEGACY
        public async Task<User> AuthByToken(string fetchToken) // this.AuthenticationWithToken was changed on AuthenticationWithCoockie
        {
            User userWithToken = await _cache.GetObjectAsync<User>(fetchToken);
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

