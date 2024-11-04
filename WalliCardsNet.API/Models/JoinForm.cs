namespace WalliCardsNet.API.Models
{
    public class JoinForm
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FieldsJson { get; set; } = "{}";
        public string CSSOptionsJson { get; set; } = "{}";
    }
}
