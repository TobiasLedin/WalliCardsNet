using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WalliCardsNet.API.Models
{
    public class Customer
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid BusinessId { get; set; }
        public Business Business { get; set; }
        public List<Device> Devices { get; set; } = [];
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        public string CustomerDetailsJson { get; set; } = "{}";

        // Exposed working property for customer details.
        [NotMapped]
        [JsonIgnore]
        public Dictionary<string, string> CustomerDetails
        {
            get => JsonSerializer.Deserialize<Dictionary<string, string>>(CustomerDetailsJson) ?? new Dictionary<string, string>();

            set => CustomerDetailsJson = JsonSerializer.Serialize(value);
        }
        // EF Core storage property.
        public string CustomerDetailsJson { get; private set; } = "{}";
    }
}
