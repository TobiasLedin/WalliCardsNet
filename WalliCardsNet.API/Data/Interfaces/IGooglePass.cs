using WalliCardsNet.API.Models;

namespace WalliCardsNet.API.Data.Interfaces
{
    public interface IGooglePass
    {
        public Task<GooglePass> GetByIdAsync(int objectId);
        public Task<List<GooglePass>> GetAllAsync();
        public Task AddAsync(GooglePass pass);
        public Task UpdateAsync(GooglePass pass);
        public Task RemoveAsync(int objectId);
    }
}
