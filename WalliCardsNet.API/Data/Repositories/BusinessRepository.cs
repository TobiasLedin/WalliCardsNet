using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Data.Models;

namespace WalliCardsNet.API.Data.Repositories
{
    public class BusinessRepository : IBusiness
    {
        public Task AddAsync(Business business)
        {
            throw new NotImplementedException("Business/AddAsync method not yet implemented");
        }

        public Task<List<Business>> GetAllAsync()
        {
            throw new NotImplementedException("Business/GetAllAsync method not yet implemented");
        }

        public Task<Business> GetByIdAsync(int id)
        {
            throw new NotImplementedException("Business/GetByIdAsync method not yet implemented");
        }

        public Task RemoveAsync(int id)
        {
            throw new NotImplementedException("Business/RemoveAsync method not yet implemented");
        }

        public Task UpdateAsync(Business business)
        {
            throw new NotImplementedException("Business/UpdateAsync method not yet implemented");
        }
    }
}
