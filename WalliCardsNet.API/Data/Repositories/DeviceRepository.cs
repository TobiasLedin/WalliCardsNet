using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Data.Models;

namespace WalliCardsNet.API.Data.Repositories
{
    public class DeviceRepository : IDevice
    {
        public Task AddAsync(Device device)
        {
            throw new NotImplementedException("Device/AddAsync method not yet implemented");
        }

        public async Task<List<Device>> GetAllAsync()
        {
            throw new NotImplementedException("Device/GetAllAsync method not yet implemented");
        }

        public Task<Device> GetByIdAsync(int id)
        {
            throw new NotImplementedException("Device/GetByIdAsync method not yet implemented");
        }

        public Task RemoveAsync(int id)
        {
            throw new NotImplementedException("Device/RemoveAsync method not yet implemented");
        }

        public Task UpdateAsync(Device device)
        {
            throw new NotImplementedException("Device/UpdateAsync method not yet implemented");
        }
    }
}
