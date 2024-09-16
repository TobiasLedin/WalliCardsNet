using System.ComponentModel.DataAnnotations;

namespace WalliCardsNet.API.Data.Models
{
    public class Business
    {
        public int Id { get; set; } 
        public string Name { get; set; } = "";
        public string PspId { get; set; } = "";
        public List<CardTemplate>? CardTemplates { get; set; }
        public List<Customer>? Customers { get; set; }
        public List<string>? CustomerDetailsJson { get; set; }

        //public ApplicationUser ApplicationUser {get; set;}
    }

    public class CustomerDetails 
    {
        public int CustomerId { get; set; }
        public string Details {  get; set; }
    }
}
