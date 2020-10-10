using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BooksTextsSplit.Models;
using BooksTextsSplit.Helpers;
using BooksTextsSplit.Services;

namespace BooksTextsSplit.Services
{
    public interface IAuthService
    {
        Task<User> Authenticate(string username, string password);
        Task<User> AuthByToken(string authKey);
        Task<IEnumerable<User>> GetAll();
    }
    public class AuthService : IAuthService
    {

        // users hardcoded for simplicity, store in a db with hashed passwords in production applications
        private List<User> _users = new List<User>
        {
            new User { 
                Id = 1, 
                FirstName = "Yuri", 
                LastName = "Gonchar", 
                Username = "YUGR",
                Token = "1234567890",
                Password = "ttt", 
                Email = "yuri.gonchar@gmail.com" }                
        };

        public async Task<User> Authenticate(string email, string password)
        {
            var user = await Task.Run(() => _users.SingleOrDefault(x => x.Email == email && x.Password == password));

            // return null if user not found
            if (user == null)
                return null;

            // authentication successful so return user details without password
            return user.WithoutPassword();
        }

        public async Task<User> AuthByToken(string fetchToken)
        {
            var authedUser = await Task.Run(() => _users.SingleOrDefault(x => x.Token == fetchToken));

            // return null if user not found
            if (authedUser == null)
                return null;

            // authentication successful so return user details without password
            return authedUser.WithoutPassword();
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            return await Task.Run(() => _users.WithoutPasswords());
        }
    }
}

