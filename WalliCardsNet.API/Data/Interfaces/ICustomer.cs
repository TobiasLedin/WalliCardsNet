using WalliCardsNet.API.Models;

namespace WalliCardsNet.API.Data.Interfaces
{
    public interface ICustomer
    {
        public Task<Customer> GetByIdAsync(int id);
        public Task<List<Customer>> GetAllAsync();
        public Task AddAsync(Customer customer);
        public Task UpdateAsync(Customer customer);
        public Task RemoveAsync(int id);
    }
}
