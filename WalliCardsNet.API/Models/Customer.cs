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

        [NotMapped]
        [JsonIgnore]
        public Dictionary<string, object> CustomerDetails
        {
            get => JsonSerializer.Deserialize<Dictionary<string, object>>(CustomerDetailsJson) ?? new Dictionary<string, object>();

            set => CustomerDetailsJson = JsonSerializer.Serialize(value);
        }
        // Final storage of Customer details
        public string CustomerDetailsJson { get; private set; } = "{}";
    }
}
