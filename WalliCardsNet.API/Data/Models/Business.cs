using System.ComponentModel.DataAnnotations;

namespace WalliCardsNet.API.Data.Models
{
    public class Business
    {
        public Guid Id { get; set; } 
        public string Name { get; set; } = "";
        public string PspId { get; set; } = "";
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        public List<CardTemplate>? CardTemplates { get; set; }
        public List<Customer>? Customers { get; set; }
        public List<string>? CustomerDetailsJson { get; set; }

        public List<ApplicationUser> ApplicationUsers { get; set; } = [];
    }

    public class CustomerDetails 
    {
        public int CustomerId { get; set; }
        public string Details {  get; set; }
    }
}
