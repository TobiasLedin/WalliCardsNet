namespace WalliCardsNet.API.Models
{
    public class PaymentEvent
    {
        public required string EventId { get; set; }
        public required string PaymentServiceProvider { get; set; }
        public required string EventType { get; set; }
        public required object EventData { get; set; }
        public required DateTime Received { get; set; }
    }
}
