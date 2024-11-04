namespace WalliCardsNet.API.Models
{
    public class JoinForm
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required Guid BusinessProfileId { get; set; }
        public string FieldsJson { get; set; } = "{}";
        public string CSSOptionsJson { get; set; } = "{}";
    }
}
