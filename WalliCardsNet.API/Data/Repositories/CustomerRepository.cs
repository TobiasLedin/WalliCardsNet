using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Helpers;
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
                customer.CustomerDetailsJson = await EncryptionHelper.EncryptAsync(JsonSerializer.Serialize(customer.CustomerDetails));

                await _applicationDbContext.Customers.AddAsync(customer);
                await _applicationDbContext.SaveChangesAsync();
            }
        }

        public async Task<List<Customer>> GetAllByBusinessAsync(Guid businessId)
        {
            if (businessId != Guid.Empty)
            {
                var customers = await _applicationDbContext.Customers
                    .Where(x => x.BusinessId == businessId)
                    .ToListAsync();

                foreach (var customer in customers)
                {
                    var decryptedJson = await EncryptionHelper.DecryptAsync(customer.CustomerDetailsJson);
                    customer.CustomerDetails = JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedJson) ?? new Dictionary<string, string>();
                }
                return customers;
            }
            return new List<Customer>();
        }

        public async Task<Customer?> GetByIdAsync(Guid id)
        {
            var customer = await _applicationDbContext.Customers.FindAsync(id);
            if (customer != null)
            {
                var decryptedJson = await EncryptionHelper.DecryptAsync(customer.CustomerDetailsJson);
                customer.CustomerDetails = JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedJson) ?? new Dictionary<string, string>();
            }
            return customer;
        }

        public async Task RemoveAsync(Guid id)
        {
            var customer = await GetByIdAsync(id);
            if (customer != null)
            {
                _applicationDbContext.Customers.Remove(customer);
                await _applicationDbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(Customer customer) // Customer.details "tappas"/återgår till ursprung efter GetById().
        {
            if (customer != null)
            {
                var updatedDetails = customer.CustomerDetails; // Persist CustomerDetails past GetByIdAsync().

                var existingCustomer = await GetByIdAsync(customer.Id);
                if (existingCustomer != null)
                {
                    existingCustomer.CustomerDetailsJson = await EncryptionHelper.EncryptAsync(JsonSerializer.Serialize(updatedDetails));

                    await _applicationDbContext.SaveChangesAsync();
                }
            }
        }
    }
}
