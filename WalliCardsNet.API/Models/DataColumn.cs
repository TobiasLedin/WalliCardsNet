using System.ComponentModel.DataAnnotations.Schema;

namespace WalliCardsNet.API.Models
{
    public class DataColumn
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [ForeignKey(nameof(Business))]
        public Guid BusinessId { get; set; }


        public string ColumnName { get; set; } = string.Empty;   // Customer data fields (Name, Email, Adress etc)
        public string ColumnType { get; set; } = "string";   // Type of data (string, int/double etc)
        public bool IsRequired { get; set; }
    }
}
