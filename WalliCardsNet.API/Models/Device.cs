namespace WalliCardsNet.API.Models
{
    public class Device
    {
        public Guid Id { get; set; }
        public required Guid CustomerId { get; set; }
        public required DeviceType Type { get; set; }
    }

    public enum DeviceType
    {
        Android = 0,
        iOS = 1,
    }
}
