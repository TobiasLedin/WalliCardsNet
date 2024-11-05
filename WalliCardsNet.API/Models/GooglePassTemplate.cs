namespace WalliCardsNet.API.Models
{
    public class GooglePassTemplate
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required Guid BusinessProfileId { get; set; }
        public required string GoogleId { get; set; }
        public required string IssuerId { get; set; }
        public required string ClassId { get; set; }
        public required string ObjectId { get; set; } // TODO: Snacka om...

        // Addition required props...
        public string CardTitle { get; set; } // Added
        public string Header { get; set; } // Added
        public required string HexBackgroundColor { get; set; }
        public string? LogoUri { get; set; } // Set optional
        public string? WideLogoUri { get; set; } // Set optional
        public string? HeroImageUri { get; set; } // Set optional
        public required string FieldsJson { get; set; }
    }
}
