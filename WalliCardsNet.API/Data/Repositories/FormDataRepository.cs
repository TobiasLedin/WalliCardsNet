using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Models;

namespace WalliCardsNet.API.Data.Repositories
{
    public class FormDataRepository : IFormData
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public FormDataRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task AddAsync(FormData data)
        {
            await _applicationDbContext.FormData.AddAsync(data);

            await _applicationDbContext.SaveChangesAsync();
        }

        public async Task<FormData> GetByIdAsync(string id)
        {
            var data = await _applicationDbContext.FormData.FindAsync(id);

            return data;
        }

        public async Task RemoveAsync(string id)
        {
            var formData = await _applicationDbContext.FormData.FindAsync(id);

            if (formData != null)
            {
                _applicationDbContext.FormData.Remove(formData);

                await _applicationDbContext.SaveChangesAsync();
            }
        }
    }
}
