namespace BooksTextsSplit.Library.Models
{
    public class LoginAttemptResult
    {
        public string ResultMessage { get; set; }
        public string[] FieldsErrors { get; set; }
        public UserData AuthUser { get; set; }
        public string IssuedToken { get; set; }
        public int ResultCode { get; set; }
    }
}
