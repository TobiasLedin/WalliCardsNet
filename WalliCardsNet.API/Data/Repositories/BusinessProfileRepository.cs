using Microsoft.EntityFrameworkCore;
using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Models;

namespace WalliCardsNet.API.Data.Repositories
{
    public class BusinessProfileRepository : IBusinessProfile
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public BusinessProfileRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
        public async Task AddAsync(BusinessProfile businessProfile)
        {
            if (businessProfile != null)
            {
                await _applicationDbContext.Profiles.AddAsync(businessProfile);
                await _applicationDbContext.SaveChangesAsync();
            }
        }

        public async Task<List<BusinessProfile>> GetAllAsync(Guid businessId)
        {
            var result = await _applicationDbContext.Profiles
                .Where(x => x.BusinessId == businessId)
                .Include(x => x.GoogleTemplate)
                .Include(x => x.JoinForm)
                .ToListAsync();
            if (result != null && result.Count > 0)
            {
                return result;
            }
            return null;
        }

        public async Task<BusinessProfile> GetByIdAsync(Guid id)
        {
            return await _applicationDbContext.Profiles.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<BusinessProfile> GetByBusinessIdAsync(Guid businessId)
        {
            var result = await _applicationDbContext.Profiles.FirstOrDefaultAsync(x => x.BusinessId == businessId && x.IsActive == true);
            if (result != null)
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        public Task RemoveAsync(int id)
        {
            throw new NotImplementedException("CardTemplate/RemoveAsync method not yet implemented");
        }

        public Task UpdateAsync(BusinessProfile businessProfile)
        {
            throw new NotImplementedException("CardTemplate/UpdateAsync method not yet implemented");
        }

        public async Task SetActiveAsync(Guid id)
        {
            var businessProfile = await _applicationDbContext.Profiles.FirstOrDefaultAsync(x => x.Id == id);
            var currentActiveProfile = await _applicationDbContext.Profiles.FirstOrDefaultAsync (x =>  x.IsActive == true);
            if (currentActiveProfile != null && currentActiveProfile != businessProfile)
            {
                currentActiveProfile.IsActive = false;
            }
            if (businessProfile != null && !businessProfile.IsActive)
            {
                businessProfile.IsActive = true;
            }
            await _applicationDbContext.SaveChangesAsync();
        }
    }
}
