namespace WalliCardsNet.API.Data.Models
{
    public class FormData
    {
        public string Id { get; set; } = new Guid().ToString();
        public required string BusinessId { get; set; }
        public required string Email { get; set; }

    }
}
