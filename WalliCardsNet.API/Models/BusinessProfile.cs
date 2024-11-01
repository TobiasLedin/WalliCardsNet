using Stripe.Issuing;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WalliCardsNet.API.Models
{
    public class BusinessProfile
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid BusinessId { get; set; }

        public required string HexBackgroundColor { get; set; }
        public required string ImageUri { get; set; }
        public string DesignJson { get; set; } = "{}";
        public bool IsActive { get; set; }

        [Required]
        public GooglePassTemplate? GoogleTemplate { get; set; }

        // Awaiting Apple Pass implementation
        //public ApplePassTemplate? ApplePassTemplate { get; set; }

    }
}
