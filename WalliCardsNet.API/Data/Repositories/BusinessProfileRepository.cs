using Microsoft.EntityFrameworkCore;
using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Models;
using WalliCardsNet.API.Services;
using WalliCardsNet.ClassLibrary.BusinessProfile;
using WalliCardsNet.ClassLibrary.Services;

namespace WalliCardsNet.API.Data.Repositories
{
    public class BusinessProfileRepository : IBusinessProfile
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IAPIBusinessProfilesService _businessProfilesService;

        public BusinessProfileRepository(ApplicationDbContext applicationDbContext, IAPIBusinessProfilesService businessProfilesService)
        {
            _applicationDbContext = applicationDbContext;
            _businessProfilesService = businessProfilesService;
        }
        
        public async Task AddAsync(BusinessProfile businessProfile)
        {
            if (businessProfile != null)
            {
                await _applicationDbContext.Profiles.AddAsync(businessProfile);
                await _applicationDbContext.SaveChangesAsync();
            }
        }

        public async Task<List<BusinessProfile>?> GetAllAsync(Guid businessId)
        {
            return await _applicationDbContext.Profiles
                .Where(x => x.BusinessId == businessId)
                .Include(x => x.GoogleTemplate)
                .Include(x => x.JoinForm)
                .ToListAsync();
        }

        public async Task<BusinessProfile?> GetByIdAsync(Guid id)
        {
            return await _applicationDbContext.Profiles
                .Include(x => x.GoogleTemplate)
                .Include(x => x.JoinForm)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<BusinessProfile?> GetActiveByBusinessIdAsync(Guid businessId)
        {
            return await _applicationDbContext.Profiles
                .Include(x => x.GoogleTemplate)
                .Include(x => x.JoinForm)
                .FirstOrDefaultAsync(x => x.BusinessId == businessId && x.IsActive == true);
        }

        public async Task RemoveAsync(Guid id)
        {
            var bp = await _applicationDbContext.Profiles.FindAsync(id);

            if (bp != null)
            {
                _applicationDbContext.Profiles.Remove(bp);
                await _applicationDbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(BusinessProfileRequestDTO businessProfile, Guid businessId)
        {
            var existingProfile = await _applicationDbContext.Profiles
                .Include (x => x.GoogleTemplate)
                .Include (x => x.JoinForm)
                .FirstOrDefaultAsync(x => x.Id == businessProfile.Id);

            if (existingProfile != null)
            {
                if (existingProfile.GoogleTemplate != null)
                {
                    existingProfile.GoogleTemplate.CardTitle = businessProfile.GooglePassTemplate.CardTitle;
                    existingProfile.GoogleTemplate.Header = businessProfile.GooglePassTemplate.Header;
                    existingProfile.GoogleTemplate.WideLogoUri = businessProfile.GooglePassTemplate.WideLogoUrl;
                    existingProfile.GoogleTemplate.UseWideLogo = businessProfile.GooglePassTemplate.UseWideLogo;
                    existingProfile.GoogleTemplate.HexBackgroundColor = businessProfile.GooglePassTemplate.HexBackgroundColor;
                    existingProfile.GoogleTemplate.LogoUri = businessProfile.GooglePassTemplate.LogoUrl;
                    existingProfile.GoogleTemplate.HeroImageUri = businessProfile.GooglePassTemplate.HeroImage;
                    existingProfile.GoogleTemplate.FieldsJson = businessProfile.GooglePassTemplate.FieldsJson;
                }

                if (existingProfile.JoinForm != null)
                {
                    existingProfile.JoinForm.Title = businessProfile.JoinFormTemplate.Title;
                    existingProfile.JoinForm.LogoUrl = businessProfile.JoinFormTemplate.LogoUrl;
                    existingProfile.JoinForm.WideLogoUrl = businessProfile.JoinFormTemplate.WideLogoUrl;
                    existingProfile.JoinForm.UseWideLogo = businessProfile.JoinFormTemplate.UseWideLogo;
                    existingProfile.JoinForm.HeroImageUrl = businessProfile.JoinFormTemplate.HeroImageUrl;
                    existingProfile.JoinForm.FieldsJson = businessProfile.JoinFormTemplate.FieldsJson;
                    existingProfile.JoinForm.CSSOptionsJson = businessProfile.JoinFormTemplate.CSSOptionsJson;
                }

                _applicationDbContext.Profiles.Update(existingProfile);
                await _applicationDbContext.SaveChangesAsync();
            }
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
