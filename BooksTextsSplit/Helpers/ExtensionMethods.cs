using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BooksTextsSplit.Models;

namespace BooksTextsSplit.Helpers
{
    public static class ExtensionMethods
    {
        public static IEnumerable<User> WithoutPasswords(this IEnumerable<User> users)
        {
            return users.Select(x => x.WithoutPassword());
        }

        public static User WithoutPassword(this User user)
        {
            if (user == null)
            {
                Console.WriteLine("\n\n Users data is expired - can't return UserWithoutPassword() ");
                return null;
            }
                user.Password = null;
                user.Token = null;
                return user;            
        }
    }
}
