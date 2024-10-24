namespace WalliCardsNet.API.Models
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public ApplicationUser? User { get; set; }
        public Business? Business { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? Details { get; set; }
    }
}
