﻿using WalliCardsNet.API.Models;

namespace WalliCardsNet.API.Data.Interfaces
{
    public interface IRefreshToken
    {
        Task AddAsync(RefreshToken token);
        Task RemoveAsync(string token);
        Task<RefreshToken?> GetAsync(string tokenString);
        Task<List<RefreshToken>> GetAllAsync();
        Task RemoveExpiredAsync();
    }
}