namespace WalliCardsNet.API.Models
{
    public class GooglePassTemplate
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required Guid BusinessId { get; set; }
        public required  string GoogleId { get; set; }
        public required string IssuerId { get; set; }
        public required string ClassId { get; set; }
        public required string ObjectId { get; set; }

        // Addition required props...
    }
}
