namespace UserApi.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public string CreatedAt { get; set; }
        public string Hobbies { get; set; } // i think it should be an object or long string will come back to this
       // public bool IsAdmin { get; set; }
        public bool IsActive { get; set; }



    }
}
