using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WalliCardsNet.API.Models
{
    public class GooglePassTemplate
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid BusinessProfileId { get; set; }
        public string? FieldsJson { get; set; } // Generic class + Generic object


        // Generic object required props
        public string? CardTitle { get; set; }
        public string? Header { get; set; }
        public string? HexBackgroundColor { get; set; }
        public string? LogoUri { get; set; }
        public string? WideLogoUri { get; set; }
        public string? HeroImageUri { get; set; }

    }
}
