namespace WalliCardsNet.API.Data.Models
{
    public class CardTemplate
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required Business Business { get; set; }
        public string DesignJson { get; set; } = "";
    }
}
