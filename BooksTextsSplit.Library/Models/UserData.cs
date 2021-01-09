using Newtonsoft.Json;
using System.Collections.Generic;

namespace BooksTextsSplit.Library.Models
{
    // добавить запись пользователей в базу из кэша, в кэше хранить копии без паролей
    public class UserData
    {
        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }
        
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "lastName")] 
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "username")]
        public string Username { get; set; }

        [JsonProperty(PropertyName = "password")]
        public string Password { get; set; }

        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "aboutMe")]
        public string AboutMe { get; set; }

        [JsonProperty(PropertyName = "photoPath")]
        public string PhotoPath { get; set; }

        [JsonProperty(PropertyName = "photoSmallPath")]
        public string PhotoSmallPath { get; set; }

        [JsonProperty(PropertyName = "userEditedBooks")]
        public List<UserEditedBook> UserEditedBooks { get; set; }

    }
    public class UserEditedBook
    {
        [JsonProperty(PropertyName = "editedBookId")]
        public int EditedBookId { get; set; }

        [JsonProperty(PropertyName = "editedBookVersion")]
        public int EditedBookVersion { get; set; }

        [JsonProperty(PropertyName = "editedBook")]
        public string EditedBookData { get; set; }
    }
}
