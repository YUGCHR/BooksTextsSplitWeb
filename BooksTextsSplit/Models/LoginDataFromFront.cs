using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BooksTextsSplit.Models
{
    public class LoginDataFromFront
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
        public string Captcha { get; set; }
    }
}