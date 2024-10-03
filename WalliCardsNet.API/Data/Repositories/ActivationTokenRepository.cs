using Microsoft.EntityFrameworkCore;
using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Models;

namespace WalliCardsNet.API.Data.Repositories
{
    public class ActivationTokenRepository : IActivationToken
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public ActivationTokenRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<ActivationToken> GetTokenAsync(Guid businessId)
        {
            var token = await _applicationDbContext.ActivationTokens.FirstOrDefaultAsync(x => x.Business.Id == businessId);

            if (token != null)
            {
                if (token.ExpirationTime <= DateTime.UtcNow)
                {
                    await DeleteTokenAsync(token.Id);
                }
                else
                {
                    return token;
                }
            }
            return await GenerateTokenAsync(businessId);
        }

        private async Task AddTokenAsync(ActivationToken token)
        {
            if (token != null)
            {
                await _applicationDbContext.ActivationTokens.AddAsync(token);
                await _applicationDbContext.SaveChangesAsync();
            }
        }
        private async Task<ActivationToken> GenerateTokenAsync(Guid businessId)
        {
            var business = await _applicationDbContext.Businesses.FirstOrDefaultAsync(x => x.Id == businessId);
            if (business == null)
            {
                throw new InvalidOperationException("Business not found");
            }
            var token = new ActivationToken
            {
                Business = business,
                ExpirationTime = DateTime.UtcNow.AddDays(1)
            };
            return token;
        }

        private async Task DeleteTokenAsync(Guid id)
        {
            var token = await _applicationDbContext.ActivationTokens.FirstOrDefaultAsync(x => x.Id == id);
            if (token != null)
            {
                _applicationDbContext.ActivationTokens.Remove(token);
                await _applicationDbContext.SaveChangesAsync();
            }
        }
    }
}
