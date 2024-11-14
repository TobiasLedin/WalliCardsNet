using WalliCardsNet.API.Models;

namespace WalliCardsNet.API.Data.Interfaces
{
    public interface IGooglePass
    {
        public Task<GooglePass?> GetByIdAsync(string objectId);
        public Task<List<GooglePass>> GetAllByClassIdAsync(string classId);
        public Task AddAsync(GooglePass pass);
        public Task UpdateAsync(GooglePass pass);
        public Task RemoveAsync(int objectId);
    }
}
