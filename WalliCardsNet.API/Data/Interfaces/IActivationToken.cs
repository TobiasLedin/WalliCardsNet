﻿using WalliCardsNet.API.Models;

namespace WalliCardsNet.API.Data.Interfaces
{
    public interface IActivationToken
    {
        public Task<ActivationToken> GetTokenAsync(string applicationUserId);
        public Task<ActivationToken> GetTokenByEmailAsync(string email);
    }
}
