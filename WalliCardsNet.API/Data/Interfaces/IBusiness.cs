using WalliCardsNet.API.Data.Models;

namespace WalliCardsNet.API.Data.Interfaces
{
    public interface IBusiness
    {
        public Task<Business> GetByIdAsync(string id);
        public Task<List<Business>> GetAllAsync();
        public Task AddAsync(Business business);
        public Task UpdateAsync(Business business);
        public Task RemoveAsync(string id);
    }
}
