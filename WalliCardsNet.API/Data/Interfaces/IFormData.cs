using WalliCardsNet.API.Data.Models;

namespace WalliCardsNet.API.Data.Interfaces
{
    public interface IFormData
    {
        Task AddAsync(FormData data);

        Task<FormData> GetByIdAsync(string id);

        Task RemoveAsync(string id);
        
    }
}