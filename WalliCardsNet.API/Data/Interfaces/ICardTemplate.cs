﻿using WalliCardsNet.API.Data.Models;

namespace WalliCardsNet.API.Data.Interfaces
{
    public interface ICardTemplate
    {
        public CardTemplate GetByIdAsync(int id);
        public List<CardTemplate> GetAllAsync();
        public Task AddAsync(CardTemplate cardTemplate);
        public Task UpdateAsync(CardTemplate cardTemplate);
        public Task RemoveAsync(int id);
    }
}