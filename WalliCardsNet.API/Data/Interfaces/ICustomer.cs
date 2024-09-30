using WalliCardsNet.API.Models;

namespace WalliCardsNet.API.Data.Interfaces
{
    public interface ICustomer
    {
        public Task<Customer> GetByIdAsync(Guid id);
        public Task<List<Customer>> GetAllByBusinessAsync(Guid businessId);
        public Task AddAsync(Customer customer);
        public Task UpdateAsync(Customer customer);
        public Task RemoveAsync(Guid id);
    }
}
