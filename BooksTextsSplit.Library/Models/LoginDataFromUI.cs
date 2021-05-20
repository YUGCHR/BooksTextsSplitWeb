using System.ComponentModel.DataAnnotations;

namespace BooksTextsSplit.Library.Models
{
    public class LoginDataFromUI
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
        public string Captcha { get; set; }
    }
}