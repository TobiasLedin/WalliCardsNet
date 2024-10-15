using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Models;

namespace WalliCardsNet.API.Data.Repositories
{
    public class DeviceRepository : IDevice
    {
        public void AddAsync(Device device)
        {
            throw new NotImplementedException("Device/AddAsync method not yet implemented");
        }

        public List<Device> GetAllAsync()
        {
            throw new NotImplementedException("Device/GetAllAsync method not yet implemented");
        }

        public Device GetByIdAsync(int id)
        {
           throw new NotImplementedException("Device/GetByIdAsync method not yet implemented");
        }

        public void RemoveAsync(int id)
        {
            throw new NotImplementedException("Device/RemoveAsync method not yet implemented");
        }

        public void UpdateAsync(Device device)
        {
            throw new NotImplementedException("Device/UpdateAsync method not yet implemented");
        }
    }
}
