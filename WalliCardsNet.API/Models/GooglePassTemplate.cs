using System.ComponentModel.DataAnnotations;

namespace WalliCardsNet.API.Models
{
    public class GooglePassTemplate
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid BusinessProfileId { get; set; }
        public string? FieldsJson { get; set; } // Used by Generic class + Generic object


        // Generic object required props
        public string CardTitle { get; set; } = "Your cardtitle";
        public string Header { get; set; } = "Your header";
        public string? HexBackgroundColor { get; set; }
        public string? LogoUri { get; set; }
        public string? WideLogoUri { get; set; }
        public string? HeroImageUri { get; set; }
    }
}
