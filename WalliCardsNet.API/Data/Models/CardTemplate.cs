namespace WalliCardsNet.API.Data.Models
{
    public class CardTemplate
    {
        public int Id { get; set; }
        public Business Business { get; set; }
        public string DesignJson { get; set; } = "";
    }
}
