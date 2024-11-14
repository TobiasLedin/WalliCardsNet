using WalliCardsNet.API.Models;
using WalliCardsNet.ClassLibrary.BusinessProfile;

namespace WalliCardsNet.API.Data.Interfaces
{
    public interface IBusinessProfileRepo
    {
        public Task AddAsync(BusinessProfile businessProfile);

        public Task<List<BusinessProfile>?> GetAllAsync(Guid businessId);

        public Task<BusinessProfile?> GetByIdAsync(Guid id);

        public Task<BusinessProfile?> GetActiveByBusinessIdAsync(Guid businessId);
        public Task RemoveAsync(Guid id);

        public Task UpdateAsync(BusinessProfileRequestDTO businessProfile, Guid businessId);
        public Task SetActiveAsync(Guid id);
    }
}
