namespace WalliCardsNet.API.Models
{
    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required Guid UserId { get; set; }
        public required string Token { get; set; }
        public required DateTime Expiry { get; set; }
    }
}
