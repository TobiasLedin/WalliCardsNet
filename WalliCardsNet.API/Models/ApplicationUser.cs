using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WalliCardsNet.API.Models
{
    // Custom user class extending IdentityUser
    public class ApplicationUser : IdentityUser
    {
        [ForeignKey(nameof(Business))]
        public Guid BusinessId { get; set; }

        [JsonIgnore]
        public Business? Business { get; set; }
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

    }
}
