using Microsoft.EntityFrameworkCore;
using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Models;

namespace WalliCardsNet.API.Data.Repositories
{
    public class GooglePassRepository : IGooglePassRepo
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public GooglePassRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task AddAsync(GooglePass pass)
        {
            await _applicationDbContext.GooglePasses.AddAsync(pass);
            await _applicationDbContext.SaveChangesAsync();
        }

        public async Task<List<GooglePass>> GetAllByClassIdAsync(string classId)
        {
            return await _applicationDbContext.GooglePasses
                .Where(gp => gp.ClassId == classId)
                .Include(gp => gp.Customer)
                .ToListAsync();
        }

        public async Task<GooglePass?> GetByIdAsync(string objectId)
        {
            return await _applicationDbContext.GooglePasses.FindAsync(objectId);
        }

        public Task RemoveAsync(int id)
        {
            throw new NotImplementedException("Device/RemoveAsync method not yet implemented");
        }

        public async Task UpdateAsync(GooglePass pass)
        {
            _applicationDbContext.GooglePasses.Update(pass);
            await _applicationDbContext.SaveChangesAsync();
        }
    }
}
