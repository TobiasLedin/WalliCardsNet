using System.Text.Json.Serialization;

namespace WalliCardsNet.API.Models
{
    public class GoogleWalletCallback
    {
        [JsonPropertyName("classId")]
        public string ClassId { get; set; } = string.Empty;

        [JsonPropertyName("objectId")]
        public string ObjectId { get; set; } = string.Empty;

        [JsonPropertyName("expTimeMillis")]
        public long ExpTimeMillis { get; set; }

        [JsonPropertyName("eventType")]
        public string EventType { get; set; } = string.Empty;

        [JsonPropertyName("nonce")]
        public string Nonce { get; set; } = string.Empty;

        // Helper methods
        public bool IsExpired() =>
            DateTimeOffset.FromUnixTimeMilliseconds(ExpTimeMillis) < DateTimeOffset.UtcNow;

        public bool IsSaveEvent() =>
            EventType.Equals("save", StringComparison.OrdinalIgnoreCase);

        public bool IsDeleteEvent() =>
            EventType.Equals("del", StringComparison.OrdinalIgnoreCase);

        public (string IssuerId, string ClassIdentifier) ParseClassId()
        {
            var parts = ClassId.Split('.');
            return parts.Length == 2
                ? (parts[0], parts[1])
                : (string.Empty, string.Empty);
        }

        public (string IssuerId, string ObjectIdentifier) ParseObjectId()
        {
            var parts = ObjectId.Split('.');
            return parts.Length == 2
                ? (parts[0], parts[1])
                : (string.Empty, string.Empty);
        }
    }
}
