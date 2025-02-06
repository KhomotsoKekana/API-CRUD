using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace UserApi.Models.Dto
{
    public class UpdateUser
    {
        [JsonIgnore]
        public int UserId { get; set; }
        public string Name { get; set; }
        public string UserName { get; set;}
        [EmailAddress]
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public string Hobbies { get; set; }
        public bool IsActive { get; set; }
    }
}
