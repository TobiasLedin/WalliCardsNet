namespace WalliCardsNet.Client.Models
{
    public class CardField
    {
        public string FieldName { get; set; }
        public string FieldType { get; set; }
        public string? Label { get; set; }
        public bool IsRequired { get; set; }
    }
}
