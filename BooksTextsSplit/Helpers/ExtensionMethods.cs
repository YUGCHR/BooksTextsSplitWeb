using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BooksTextsSplit.Models;

namespace BooksTextsSplit.Helpers
{
    public static class ExtensionMethods
    {
        public static IEnumerable<UserData> WithoutPasswords(this IEnumerable<UserData> users)
        {
            return users.Select(x => x.WithoutPassword());
        }

        public static UserData WithoutPassword(this UserData user)
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
