using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Data.Models;

namespace WalliCardsNet.API.Data.Repositories
{
    public class CustomerRepository : ICustomer
    {
        public Task AddAsync(Customer customer)
        {
            throw new NotImplementedException();
        }

        public List<Customer> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Customer GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Customer customer)
        {
            throw new NotImplementedException();
        }
    }
}
