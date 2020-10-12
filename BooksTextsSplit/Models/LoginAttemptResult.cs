using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BooksTextsSplit.Models
{
    public class LoginAttemptResult
    {
        public string ResultMessage { get; set; }
        public string[] FieldsErrors { get; set; }
        public User AuthUser { get; set; }
        public string IssuedToken { get; set; }
        public int ResultCode { get; set; }
    }
}
