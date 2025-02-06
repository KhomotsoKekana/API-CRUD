using System.Text.Json.Serialization;

namespace UserApi.Models.Dto
{
    public class Login
    {
        [JsonIgnore]
 
        
        
        public int UserId { get; set; }
      //  public string? Name { get; set; }
      //  public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}
