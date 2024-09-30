using Microsoft.EntityFrameworkCore;
using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Models;

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
                var business = await _applicationDbContext.Businesses.FindAsync(customer.BusinessId);
                if (business == null)
                {
                    throw new KeyNotFoundException("No existing business with the supplied id");
                };

                customer.Business = business;
                customer.Devices = null;
                await _applicationDbContext.Customers.AddAsync(customer);
            }
            await _applicationDbContext.SaveChangesAsync();
        }

        public async Task<List<Customer>> GetAllByBusinessAsync(Guid businessId)
        {
            var customers = new List<Customer>();

            if (businessId != null)
            {
                customers = await _applicationDbContext.Customers.Where(x => x.BusinessId == businessId).ToListAsync();
            }

            return customers;
        }

        public Task<Customer> GetByIdAsync(Guid id)
        {
            throw new NotImplementedException("Customer/GetByIdAsync method not yet implemented");
        }

        public Task RemoveAsync(Guid id)
        {
            throw new NotImplementedException("Customer/RemoveAsync method not yet implemented");
        }

        public Task UpdateAsync(Customer customer)
        {
            throw new NotImplementedException("Customer/UpdateAsync method not yet implemented");
        }
    }
}
