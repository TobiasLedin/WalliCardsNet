using Microsoft.EntityFrameworkCore;
using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Models;

namespace WalliCardsNet.API.Data.Repositories
{
    public class CardTemplateRepository : ICardTemplate
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public CardTemplateRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
        public async Task AddAsync(CardTemplate cardTemplate)
        {
            if (cardTemplate != null)
            {
                await _applicationDbContext.CardTemplates.AddAsync(cardTemplate);
                await _applicationDbContext.SaveChangesAsync();
            }
        }

        public Task<List<CardTemplate>> GetAllAsync()
        {
            throw new NotImplementedException("CardTemplate/GetAllAsync method not yet implemented");
        }

        public Task<CardTemplate> GetByIdAsync(int id)
        {
            throw new NotImplementedException("CardTemplate/GetByIdAsync method not yet implemented");
        }

        public async Task<CardTemplate> GetByBusinessIdAsync(Guid businessId)
        {
            var result = await _applicationDbContext.CardTemplates.FirstOrDefaultAsync(x => x.Business.Id == businessId && x.IsActive == true);
            if (result != null)
            {
                return result;
            }
            else
            {
                return null;
            }
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
