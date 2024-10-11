
using System.ComponentModel.DataAnnotations.Schema;

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
        public List<CardTemplate> CardTemplates { get; set; } = [];
        public List<DataColumn> DataColumns { get; set; } = [];
        public List<Customer> Customers { get; set; } = [];
        public List<ApplicationUser> ApplicationUsers { get; set; } = []; // Managers and Employees with access to client application.
    }

    public enum Status
    {
        Cancelled = 0,
        Active = 1,
        PaymentFailed = 2
    }

}
