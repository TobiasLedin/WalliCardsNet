namespace WalliCardsNet.API.Models
{
    public class FormData
    {
        public Guid Id { get; set; } = new Guid();
        public required string BusinessId { get; set; }
        public required string Email { get; set; }

    }
}
