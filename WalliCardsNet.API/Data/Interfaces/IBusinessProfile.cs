using WalliCardsNet.API.Models;

namespace WalliCardsNet.API.Data.Interfaces
{
    public interface IBusinessProfile
    {
        public Task AddAsync(BusinessProfile businessProfile);

        public Task<List<BusinessProfile>> GetAllAsync(Guid businessId);

        public Task<BusinessProfile> GetByIdAsync(Guid id);

        public Task<BusinessProfile> GetByBusinessIdAsync(Guid businessId);
        public Task RemoveAsync(int id);

        public Task UpdateAsync(BusinessProfile businessProfile);
        public Task SetActiveAsync(Guid id);
    }
}
