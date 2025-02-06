using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace UserApi.Models.Dto
{
    public class AddUser
    {
        [JsonIgnore]
        public int UserId { get; set; }
        public string? Name { get; set; }
        [Required,StringLength(50)]
        public string? UserName { get; set; }
        [EmailAddress,Required]
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? PhoneNumber { get; set; }
      //  public string CreatedAt { get; set; }
        public string? Hobbies { get; set; }
        public bool IsActive { get; set; }
    }
}
