using WalliCardsNet.API.Models;

namespace WalliCardsNet.API.Data.Interfaces
{
    public interface ICardTemplate
    {
        public Task<CardTemplate> GetByIdAsync(int id);
        public Task<CardTemplate> GetByBusinessIdAsync(Guid businessId);
        public Task<List<CardTemplate>> GetAllAsync();
        public Task AddAsync(CardTemplate cardTemplate);
        public Task UpdateAsync(CardTemplate cardTemplate);
        public Task RemoveAsync(int id);
    }
}
