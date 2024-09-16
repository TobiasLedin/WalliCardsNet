using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Data.Models;

namespace WalliCardsNet.API.Data.Repositories
{
    public class DeviceRepository : IDevice
    {
        public Task AddAsync(Device device)
        {
            throw new NotImplementedException();
        }

        public List<Device> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Device GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Device device)
        {
            throw new NotImplementedException();
        }
    }
}
