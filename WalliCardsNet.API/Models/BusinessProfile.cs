using System.ComponentModel.DataAnnotations;

namespace WalliCardsNet.API.Models
{
    public class BusinessProfile
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid BusinessId { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public string DesignJson { get; set; } = "{}";
        public bool IsActive { get; set; }

        [Required]
        public GooglePassTemplate? GoogleTemplate { get; set; }

        [Required]
        public JoinForm? JoinForm { get; set; }

        // Awaiting Apple Pass implementation
        //public ApplePassTemplate? ApplePassTemplate { get; set; }

    }
}
