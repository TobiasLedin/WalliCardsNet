using WalliCardsNet.API.Data.Models;

namespace WalliCardsNet.API.Data.Interfaces
{
    public interface ICardTemplate
    {
        public Task<CardTemplate> GetByIdAsync(int id);
        public Task<List<CardTemplate>> GetAllAsync();
        public Task AddAsync(CardTemplate cardTemplate);
        public Task UpdateAsync(CardTemplate cardTemplate);
        public Task RemoveAsync(int id);
    }
}
