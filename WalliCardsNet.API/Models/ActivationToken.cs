namespace WalliCardsNet.API.Models
{
    public class ActivationToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public ApplicationUser ApplicationUser { get; set; }
        public DateTime ExpirationTime { get; set; }
    }
}
