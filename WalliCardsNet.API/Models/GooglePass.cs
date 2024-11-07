using System.ComponentModel.DataAnnotations;

namespace WalliCardsNet.API.Models
{
    public class GooglePass
    {
        [Key]
        public required string ObjectId { get; set; } // specifikt för end-user
        public required string ClassId { get; set; }
        public Guid CustomerId { get; set; } // FK

    }
}
