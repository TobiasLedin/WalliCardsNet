using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Data.Models;

namespace WalliCardsNet.API.Data.Repositories
{
    public class BusinessRepository : IBusiness
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public BusinessRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
        public async Task AddAsync(Business business)
        {
            if (business != null)
            {
                await _applicationDbContext.AddAsync(business);
            }
            await _applicationDbContext.SaveChangesAsync();
        }

        public async Task<List<Business>> GetAllAsync()
        {
            var result = await _applicationDbContext.Businesses.ToListAsync();
            if (result != null)
            {
                return result;
            }
            return null;
        }

        public async Task<Business> GetByIdAsync(string id)
        {
            var result = await _applicationDbContext.Businesses.FirstOrDefaultAsync(x => x.Id == id);
            if (result != null)
            {
                return result;
            }
                return null;
        }

        public async Task RemoveAsync(string id)
        {
            var business = await GetByIdAsync(id);
            _applicationDbContext.Businesses.Remove(business);
            await _applicationDbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Business business)
        {
            _applicationDbContext.Businesses.Update(business);
            await _applicationDbContext.SaveChangesAsync();
        }
    }
}
