using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Data.Models;

namespace WalliCardsNet.API.Data.Repositories
{
    public class CustomerRepository : ICustomer
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public CustomerRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task AddAsync(Customer customer)
        {
            if (customer != null)
            {
                customer.Businesses = null;
                customer.Devices = null;
                await _applicationDbContext.Customers.AddAsync(customer);
            }
            await _applicationDbContext.SaveChangesAsync();
        }

        public Task<List<Customer>> GetAllAsync()
        {
            throw new NotImplementedException("Customer/GetAllAsync method not yet implemented");
        }

        public Task<Customer> GetByIdAsync(int id)
        {
            throw new NotImplementedException("Customer/GetByIdAsync method not yet implemented");
        }

        public Task RemoveAsync(int id)
        {
            throw new NotImplementedException("Customer/RemoveAsync method not yet implemented");
        }

        public Task UpdateAsync(Customer customer)
        {
            throw new NotImplementedException("Customer/UpdateAsync method not yet implemented");
        }
    }
}
