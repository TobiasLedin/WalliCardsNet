namespace WalliCardsNet.Client.Models
{
    public class CardDesign
    {
        public List<CardField> CardFields { get; set; } = new List<CardField>();
        public Dictionary<string, string> CssOptions { get; set; } = new Dictionary<string, string>();
    }
}
