using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Data.Models;

namespace WalliCardsNet.API.Data.Repositories
{
    public class CardTemplateRepository : ICardTemplate
    {
        public Task AddAsync(CardTemplate cardTemplate)
        {
            throw new NotImplementedException();
        }

        public List<CardTemplate> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public CardTemplate GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(CardTemplate cardTemplate)
        {
            throw new NotImplementedException();
        }
    }
}
