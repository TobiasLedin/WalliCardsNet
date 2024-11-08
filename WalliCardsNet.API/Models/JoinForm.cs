namespace WalliCardsNet.API.Models
{
    public class JoinForm
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required Guid BusinessProfileId { get; set; }
        public string Title { get; set; }
        public string LogoUrl { get; set; }
        public string WideLogoUrl { get; set; }
        public string HeroImageUrl { get; set; }
        public bool UseWideLogo { get; set; }
        public string FieldsJson { get; set; } = "{}";
        public string CSSOptionsJson { get; set; } = "{}";
    }
}
