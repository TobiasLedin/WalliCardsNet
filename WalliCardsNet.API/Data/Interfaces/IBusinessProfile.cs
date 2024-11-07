using WalliCardsNet.API.Models;
using WalliCardsNet.ClassLibrary.BusinessProfile;
using WalliCardsNet.ClassLibrary.BusinessProfile.Models;

namespace WalliCardsNet.API.Data.Interfaces
{
    public interface IBusinessProfile
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
