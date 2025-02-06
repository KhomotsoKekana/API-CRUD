namespace UserApi.Models.Tokens
{
    public class RefreshToken
    {
        public string Token { get; set; }
        public int UserId { get; set; }
        public DateTime? Expiry { get; set; }
    }
}
