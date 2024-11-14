using Microsoft.EntityFrameworkCore;
using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Models;

namespace WalliCardsNet.API.Data.Repositories
{
    public class ActivationTokenRepository : IActivationTokenRepo
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public ActivationTokenRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<ActivationToken> GetTokenAsync(string applicationUserId)
        {
            var token = await _applicationDbContext.ActivationTokens.FirstOrDefaultAsync(x => x.ApplicationUser.Id == applicationUserId);
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
            return await GenerateTokenAsync(applicationUserId);
        }

        public async Task<ActivationToken> GetTokenByEmailAsync(string email)
        {
            var token = await _applicationDbContext.ActivationTokens.FirstOrDefaultAsync(x => x.ApplicationUser.Email == email);
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
            return await GenerateTokenByEmailAsync(email);
        }

        private async Task AddTokenAsync(ActivationToken token)
        {
            if (token != null)
            {
                await _applicationDbContext.ActivationTokens.AddAsync(token);
                await _applicationDbContext.SaveChangesAsync();
            }
        }
        private async Task<ActivationToken> GenerateTokenAsync(string applicationUserId)
        {
            var applicationUser = await _applicationDbContext.Users.FirstOrDefaultAsync(x => x.Id == applicationUserId);
            if (applicationUser == null)
            {
                throw new InvalidOperationException("User not found");
            }
            var token = new ActivationToken
            {
                ApplicationUser = applicationUser, 
                ExpirationTime = DateTime.UtcNow.AddDays(1)
            };
            await AddTokenAsync(token);
            return token;
        }

        private async Task<ActivationToken> GenerateTokenByEmailAsync(string email)
        {
            var applicationUser = await _applicationDbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (applicationUser == null)
            {
                throw new InvalidOperationException("User not found");
            }
            var token = new ActivationToken
            {
                ApplicationUser = applicationUser,
                ExpirationTime = DateTime.UtcNow.AddDays(1)
            };
            await AddTokenAsync(token);
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
