using WalliCardsNet.API.Models;

namespace WalliCardsNet.API.Data.Interfaces
{
    public interface IDevice
    {
        Device GetByIdAsync(int id);
        List<Device> GetAllAsync();
        void AddAsync(Device device);
        void UpdateAsync(Device device);
        void RemoveAsync(int id);
    }
}
