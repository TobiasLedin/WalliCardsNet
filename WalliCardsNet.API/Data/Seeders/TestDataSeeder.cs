using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Models;
using WalliCardsNet.ClassLibrary.Business;

namespace WalliCardsNet.API.Data.Seeders
{
    public class TestDataSeeder
    {

        public static async Task SeedAsync(ApplicationDbContext appDbContext, UserManager<ApplicationUser> userManager, ICustomer customerRepo)
        {

            if (await appDbContext.Businesses.FirstOrDefaultAsync(x => x.PspId == "51c667dd-a97c-41ed-a2ca-3dcc4ca3ee9c") is null)
            {
                var business = new Business
                {
                    Name = "Kulkiosken",
                    PspId = "51c667dd-a97c-41ed-a2ca-3dcc4ca3ee9c",
                    UrlToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(8))
                                        .Replace("+", "")
                                        .Replace("/", "")
                                        .Substring(0, 10)
                };

                business.ColumnPreset = new ColumnPreset { VisibleColumns = ["Name", "Email"], HiddenColumns = ["Phone", "Address"] };

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
                        CustomerDetails = new Dictionary<string, string>
                        {
                            { "Email", "john@mail.com" },
                            { "Name", "John Johnsson" },
                            { "Phone", "070-555 555" }
                        }
                    },
                    new Customer()
                    {
                        BusinessId = business.Id,
                        CustomerDetails = new Dictionary<string, string>
                        {
                            { "Email", "sven@mail.com" },
                            { "Name", "Sven Svensson" },
                            { "Phone", "070-666 666" }
                        }
                    },
                    new Customer()
                    {
                        BusinessId = business.Id,
                        CustomerDetails = new Dictionary<string, string>
                        {
                            { "Email", "lars@mail.com" },
                            { "Name", "Lars Larsson" },
                            { "Phone", "070-777 777" }
                        }
                    }
                };

                foreach (var customer in customers)
                {
                    await customerRepo.AddAsync(customer);
                }
                

            }

        }
    }
}
