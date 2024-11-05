using WalliCardsNet.API.Models;

namespace WalliCardsNet.API.Data.Interfaces
{
    public interface IBusinessProfile
    {
        public Task AddAsync(BusinessProfile businessProfile);

        public Task<List<BusinessProfile>> GetAllAsync();

        public Task<BusinessProfile> GetByIdAsync(int id);

        public Task<BusinessProfile> GetByBusinessIdAsync(Guid businessId);
        public Task RemoveAsync(int id);

        public Task UpdateAsync(CardTemplate cardTemplate);
    }
}
