namespace WalliCardsNet.ClassLibrary.BusinessProfile.Models
{
    public class GooglePassTemplate
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid BusinessProfileId { get; set; }
        public string GoogleId { get; set; }
        public string IssuerId { get; set; }
        public string ClassId { get; set; }
        public string ObjectId { get; set; }

        // Addition required props...
        public string CardTitle { get; set; } // Added
        public string Header { get; set; } // Added
        public string HexBackgroundColor { get; set; }
        public string? LogoUri { get; set; }
        public string? WideLogoUri { get; set; } // Set optional
        public string? HeroImageUri { get; set; }
        public string FieldsJson { get; set; }
    }
}
