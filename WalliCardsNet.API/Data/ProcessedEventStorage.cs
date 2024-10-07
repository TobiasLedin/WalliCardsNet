using System.Collections.Concurrent;

namespace WalliCardsNet.API.Data
{
    public class ProcessedEventStorage
    {

        private readonly ConcurrentDictionary<string, DateTime> _processedEvents;
        private readonly TimeSpan _lifeTime = TimeSpan.FromDays(7); // Set how long events will be stored.

        public ProcessedEventStorage() 
        { 
            _processedEvents = new ConcurrentDictionary<string, DateTime>();
        }

        public bool EventExists(string eventId)
        {
                DeleteExpiredEvents();
                return _processedEvents.ContainsKey(eventId);
        }

        public void MarkAsProcessed(string eventId)
        {
                DeleteExpiredEvents();
                _processedEvents[eventId] = DateTime.UtcNow;
        }

        public void DeleteExpiredEvents()
        {
            var expirationTime = DateTime.UtcNow - _lifeTime;

            foreach (var kvp in _processedEvents)
            {
                if(kvp.Value < expirationTime)
                {
                    _processedEvents.TryRemove(kvp.Key, out _);
                }
            }
        }
    }
}
