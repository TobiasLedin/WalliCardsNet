using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WalliCardsNet.API.Models
{
    public class Customer
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid BusinessId { get; set; }
        public List<Device> Devices { get; set; } = [];
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        // Working property for customer details.
        [NotMapped]
        [JsonIgnore]
        public Dictionary<string, string> CustomerDetails { get; set; } = new Dictionary<string, string>();
       
        // EF Core storage property after serialization and encryption.
        public string CustomerDetailsJson { get; set; } = "{}";
    }
}
