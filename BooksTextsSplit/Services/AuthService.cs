using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using CachingFramework.Redis;
using BooksTextsSplit.Models;
using BooksTextsSplit.Helpers;

namespace BooksTextsSplit.Services
{
    public interface IAuthService
    {
        Task<User> Authenticate(string email, string password);
        Task<User> AuthByToken(string authKey);
        //Task AuthenticateToCookie(string userName);
        //Task<IEnumerable<User>> GetAll();
    }
    public class AuthService : IAuthService
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly RedisContext cache;
        public AuthService(IHttpContextAccessor httpContext, RedisContext c)
        {
            _httpContext = httpContext;
            cache = c;            
        }

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

        public async Task<User> Authenticate(string email, string password)
        {
            //var user = await Task.Run(() => _users.SingleOrDefault(x => x.Email == email && x.Password == password));
            User user = await cache.Cache.GetObjectAsync<User>(email);


            // it is necessary to check password here!


            await AuthByCookie(email);

            // return null if user not found
            if (user == null)
                return null;

            // authentication successful so return user details without password
            return user.WithoutPassword();
        }

        public async Task<User> AuthByToken(string fetchToken) // this.AuthenticationWithToken was changed on AuthenticationWithCoockie
        {
            User userWithToken = await cache.Cache.GetObjectAsync<User>(fetchToken);
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

        //public async Task<IEnumerable<User>> GetAll()
        //{
        //    return await Task.Run(() => _users.WithoutPasswords());
        //}
    }
}

