using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Models;

namespace WalliCardsNet.API.Data.Repositories
{
    public class GooglePassRepository : IGooglePass
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public GooglePassRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public Task AddAsync(GooglePass pass)
        {
            throw new NotImplementedException("Device/AddAsync method not yet implemented");
        }

        public async Task<List<GooglePass>> GetAllAsync()
        {
            throw new NotImplementedException("Device/GetAllAsync method not yet implemented");
        }

        public Task<GooglePass> GetByIdAsync(int objectId)
        {
            throw new NotImplementedException("Device/GetByIdAsync method not yet implemented");
        }

        public Task RemoveAsync(int id)
        {
            throw new NotImplementedException("Device/RemoveAsync method not yet implemented");
        }

        public Task UpdateAsync(GooglePass pass)
        {
            throw new NotImplementedException("Device/UpdateAsync method not yet implemented");
        }
    }
}
