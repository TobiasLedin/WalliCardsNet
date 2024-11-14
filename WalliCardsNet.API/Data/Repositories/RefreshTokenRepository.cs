using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Models;

namespace WalliCardsNet.API.Data.Repositories
{
    public class RefreshTokenRepository : IRefreshToken
    {
        private readonly ApplicationDbContext _appDbCtx;

        public RefreshTokenRepository(ApplicationDbContext appDbCtx)
        {
            _appDbCtx = appDbCtx;
        }

        public async Task AddAsync(RefreshToken token)
        {
            if (token != null)
            {
                await _appDbCtx.RefreshTokens.AddAsync(token);
                await _appDbCtx.SaveChangesAsync();
            }
        }

        public async Task<RefreshToken?> GetAsync(string tokenString)
        {
            return await _appDbCtx.RefreshTokens.FirstOrDefaultAsync(x => x.Token == tokenString);
        }

        public async Task<List<RefreshToken>> GetAllAsync()
        {
            return await _appDbCtx.RefreshTokens.ToListAsync();
        }

        public async Task RemoveAsync(string tokenString)
        {
            if (!string.IsNullOrWhiteSpace(tokenString))
            {
                var refreshToken = await _appDbCtx.RefreshTokens.FirstOrDefaultAsync(x => x.Token == tokenString);
                if (refreshToken != null)
                {
                    _appDbCtx.RefreshTokens.Remove(refreshToken);
                    await _appDbCtx.SaveChangesAsync();
                }
            }
        }

        public async Task RemoveExpiredAsync()
        {
            var expired = await _appDbCtx.RefreshTokens.Where(x => x.Expiry < DateTime.UtcNow).ToListAsync();

            if (expired.Count > 0)
            {
                _appDbCtx.RemoveRange(expired);
                await _appDbCtx.SaveChangesAsync();
            }
        }
    }
}
