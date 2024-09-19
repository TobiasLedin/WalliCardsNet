namespace WalliCardsNet.API.Data.Models
{
    public class Device
    {
        public Guid Id { get; set; }
        public Customer Customer { get; set; }
        public DeviceType Type { get; set; }
    }

    public enum DeviceType
    {
        Android = 0,
        iOS = 1,
    }
}
