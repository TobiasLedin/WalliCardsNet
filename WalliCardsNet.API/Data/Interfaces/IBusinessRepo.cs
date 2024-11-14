using WalliCardsNet.API.Models;

namespace WalliCardsNet.API.Data.Interfaces
{
    public interface IBusinessRepo
    {
        public Task<Business> GetByIdAsync(Guid id);
        public Task<Business> GetByIdAsync(string pspId);
        public Task<Business> GetByTokenAsync(string token);
        public Task<List<Business>> GetAllAsync();
        public Task AddAsync(Business business);
        public Task UpdateAsync(Business business);
        public Task AddCardDesignFieldsToColumnPresetAsync(string designJson, Guid businessId);
        public Task RemoveAsync(Guid id);
        public Task<bool> BusinessWithPspIdExists(string pspId);
    }
}
