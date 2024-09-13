namespace WalliCardsNet.API.Data.Models
{
    public class Customer
    {
        public string Id { get; set; } = "";
        public List<Business>? Businesses { get; set; }
        public List<Device>? Devices { get; set; }
        public string DetailsJson { get; set; } = "";  //Hur löser vi det här? Business A har x details om kunden, men Business B kanske har en hel annan uppsättning details
    }
}
