using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WalliCardsNet.API.Models;

namespace WalliCardsNet.API.Data.Seeders
{
    public class TestDataSeeder
    {

        public static async Task SeedAsync(ApplicationDbContext appDbContext, UserManager<ApplicationUser> userManager)
        {

            if (await appDbContext.Businesses.FirstOrDefaultAsync(x => x.PspId == "51c667dd-a97c-41ed-a2ca-3dcc4ca3ee9c") is null)
            {
                var business = new Business
                {
                    Name = "Kulkiosken",
                    PspId = "51c667dd-a97c-41ed-a2ca-3dcc4ca3ee9c"
                };

                List<DataColumn> columns = new()
                {
                new DataColumn { Id = Guid.NewGuid(), BusinessId = business.Id, Key = "Email", Title="Email", DataType = "string", IsRequired = true },
                new DataColumn { Id = Guid.NewGuid(), BusinessId = business.Id, Key = "Name", Title="Full name", DataType = "string", IsRequired = false },
                new DataColumn { Id = Guid.NewGuid(), BusinessId = business.Id, Key = "Phone", Title="Mobile phone", DataType = "string", IsRequired = false }
                };

                business.DataColumns.AddRange(columns);

                await appDbContext.Businesses.AddAsync(business);
                await appDbContext.SaveChangesAsync();


                if (await userManager.FindByEmailAsync("kalle.kula@mail.com") is null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = "Kalle Kula",
                        NormalizedUserName = "KALLE KULA",
                        Email = "kalle.kula@mail.com",
                        NormalizedEmail = "KALLE.KULA@MAIL.COM",
                        EmailConfirmed = true,
                        Business = business
                    };

                    await userManager.CreateAsync(user, "Asdf1234!");
                    await userManager.AddToRoleAsync(user, Constants.Roles.Manager);
                }



                var customers = new List<Customer>
                {
                    new Customer()
                    {
                        BusinessId = business.Id,
                        Business = business,
                        CustomerDetails = new Dictionary<string, string>
                        {
                            { "Email", "john@mail.com" },
                            { "Name", "John Johnsson" }
                        }
                    },
                    new Customer()
                    {
                        BusinessId = business.Id,
                        Business = business,
                        CustomerDetails = new Dictionary<string, string>
                        {
                            { "Email", "sven@mail.com" },
                            { "Name", "Sven Svensson" }
                        }
                    },
                    new Customer()
                    {
                        BusinessId = business.Id,
                        Business = business,
                        CustomerDetails = new Dictionary<string, string>
                        {
                            { "Email", "lars@mail.com" },
                            { "Name", "Lars Larsson" }
                        }
                    }
                };

                await appDbContext.Customers.AddRangeAsync(customers);
                await appDbContext.SaveChangesAsync();

            }

        }
    }
}
