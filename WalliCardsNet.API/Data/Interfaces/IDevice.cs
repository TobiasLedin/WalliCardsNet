using WalliCardsNet.API.Models;

namespace WalliCardsNet.API.Data.Interfaces
{
    public interface IDevice
    {
        public Task<Device> GetByIdAsync(int id);
        public Task<List<Device>> GetAllAsync();
        public Task AddAsync(Device device);
        public Task UpdateAsync(Device device);
        public Task RemoveAsync(int id);
    }
}
