using WalliCardsNet.API.Models;

namespace WalliCardsNet.API.Data.Interfaces
{
    public interface IBusiness
    {
        public Task<Business> GetByIdAsync(Guid id);
        public Task<Business> GetByTokenAsync(string token);
        public Task<List<Business>> GetAllAsync();
        public Task AddAsync(Business business);
        public Task UpdateAsync(Business business);
        public Task RemoveAsync(Guid id);
    }
}
