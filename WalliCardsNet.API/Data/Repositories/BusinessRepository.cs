using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Data.Models;

namespace WalliCardsNet.API.Data.Repositories
{
    public class BusinessRepository : IBusiness
    {
        public Task AddAsync(Business business)
        {
            throw new NotImplementedException();
        }

        public List<Business> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Business GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Business business)
        {
            throw new NotImplementedException();
        }
    }
}
