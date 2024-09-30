namespace WalliCardsNet.API.Models
{
    public class CardTemplate
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required Business Business { get; set; }
        public string DesignJson { get; set; } = "{}";
        public bool IsActive { get; set;  }
    }
}
