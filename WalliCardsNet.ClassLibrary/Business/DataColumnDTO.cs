using System.ComponentModel.DataAnnotations.Schema;

namespace WalliCardsNet.ClassLibrary.Business
{
    public record DataColumnDTO(string ColumnName, string ColumnType, bool IsRequired);
    //{
    //    public string ColumnName { get; set; } = string.Empty;   // Customer data fields (Name, Email, Adress etc)
    //    public string ColumnType { get; set; } = "string";   // Type of data (string, int/double etc)
    //    public bool IsRequired { get; set; }
    //}
}
