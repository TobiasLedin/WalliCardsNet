using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
using WalliCardsNet.API.Enums;

namespace WalliCardsNet.API.Models
{
    public class GooglePass
    {
        [Key]
        public required string ObjectId { get; set; }
        public required string ObjectJson { get; set; }
        public required string ClassId { get; set; }
        public required string ClassJson { get; set; }
        public required Customer Customer { get; set; }
        public PassStatus PassStatus { get; set; } = PassStatus.Generated;

    }
 
}
