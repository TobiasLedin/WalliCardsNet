namespace WalliCardsNet.API.Data.Models
{
    public class Customer
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public List<Business>? Businesses { get; set; }
        public List<Device>? Devices { get; set; }
    }
}
