using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Models;

namespace WalliCardsNet.API.Data.Repositories
{
    public class CardTemplateRepository : ICardTemplate
    {
        public Task AddAsync(CardTemplate cardTemplate)
        {
            throw new NotImplementedException("CardTemplate/AddAsync method not yet implemented");
        }

        public Task<List<CardTemplate>> GetAllAsync()
        {
            throw new NotImplementedException("CardTemplate/GetAllAsync method not yet implemented");
        }

        public Task<CardTemplate> GetByIdAsync(int id)
        {
            throw new NotImplementedException("CardTemplate/GetByIdAsync method not yet implemented");
        }

        public Task RemoveAsync(int id)
        {
            throw new NotImplementedException("CardTemplate/RemoveAsync method not yet implemented");
        }

        public Task UpdateAsync(CardTemplate cardTemplate)
        {
            throw new NotImplementedException("CardTemplate/UpdateAsync method not yet implemented");
        }
    }
}
