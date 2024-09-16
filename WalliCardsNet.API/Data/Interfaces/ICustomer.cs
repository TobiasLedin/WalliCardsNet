using WalliCardsNet.API.Data.Models;

namespace WalliCardsNet.API.Data.Interfaces
{
    public interface ICustomer
    {
        public Customer GetByIdAsync(int id);
        public List<Customer> GetAllAsync();
        public Task AddAsync(Customer customer);
        public Task UpdateAsync(Customer customer);
        public Task RemoveAsync(int id);
    }
}
