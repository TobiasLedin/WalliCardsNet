using System.ComponentModel.DataAnnotations.Schema;

namespace WalliCardsNet.API.Models
{
    public class FieldDefinition
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [ForeignKey(nameof(Business))]
        public Guid BusinessId { get; set; }


        public string FieldName { get; set; } = string.Empty;   // Customer data fields (Name, Email, Adress etc)
        public string FieldType { get; set; } = "string";   // Type of data (string, int/double etc)
        public bool IsRequired { get; set; }
    }
}
