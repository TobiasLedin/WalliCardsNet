using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
using WalliCardsNet.ClassLibrary.Business;
using WalliCardsNet.ClassLibrary.BusinessProfile.Models;

namespace WalliCardsNet.API.Models
{
    public class Business
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UrlToken { get; set; } = "";
        public string Name { get; set; } = "";
        public string PspId { get; set; } = ""; // Stripe customer Id
        public Status? SubscriptionStatus { get; set; }
        public string SubscriptionType { get; set; } = "";
        public DateTime? SubscriptionEndDate { get; set; }
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        public List<BusinessProfile> Profiles { get; set; } = [];
        public List<CardTemplate> CardTemplates { get; set; } = []; //TODO: Obsolete
        public List<Customer> Customers { get; set; } = [];
        public List<ApplicationUser> ApplicationUsers { get; set; } = [];

        [NotMapped]
        [JsonIgnore]
        public ColumnPreset ColumnPreset
        {
            get => JsonSerializer.Deserialize<ColumnPreset>(ColumnPresetJson) ?? new ColumnPreset();
            set => ColumnPresetJson = JsonSerializer.Serialize(value);
        }
        // EF Core storage property
        public string ColumnPresetJson { get; set; } = "{}";

    }

    public enum Status
    {
        Cancelled = 0,
        Active = 1,
        PaymentFailed = 2
    }

}
