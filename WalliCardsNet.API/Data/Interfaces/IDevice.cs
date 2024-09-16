using WalliCardsNet.API.Data.Models;

namespace WalliCardsNet.API.Data.Interfaces
{
    public interface IDevice
    {
        public Device GetByIdAsync(int id);
        public List<Device> GetAllAsync();
        public Task AddAsync(Device device);
        public Task UpdateAsync(Device device);
        public Task RemoveAsync(int id);
    }
}
