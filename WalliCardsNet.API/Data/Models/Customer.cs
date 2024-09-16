namespace WalliCardsNet.API.Data.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public List<Business>? Businesses { get; set; }
        public List<Device>? Devices { get; set; }
    }
}
