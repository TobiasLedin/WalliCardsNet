using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using WalliCardsNet.API.Models;

namespace WalliCardsNet.API.Data
{
    // EF setup to use int Id's for ApplicationUser and IdentityRole

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }

        // DB sets
        public DbSet<Business> Businesses { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CardTemplate> CardTemplates { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<FormData> FormData { get; set; }
        public DbSet<ActivationToken> ActivationTokens { get; set; }

        // DB context configuration

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

        }

        // DB Model configuration

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = "3626b5c9-6d31-4664-85cf-505b25561781",
                    Name = Constants.Roles.Administrator,
                    NormalizedName = Constants.Roles.Administrator.ToUpper()
                },
                new IdentityRole
                {
                    Id = "28dbb454-33e3-445c-b68a-c8e327175158",
                    Name = Constants.Roles.Manager,
                    NormalizedName = Constants.Roles.Manager.ToUpper()
                },
                new IdentityRole
                {
                    Id = "dd207899-ce7e-4217-ab90-2ffffcfe84ef",
                    Name = Constants.Roles.Employee,
                    NormalizedName = Constants.Roles.Employee.ToUpper()
                });

            // String conversion of SubscriptionStatus enum.
            builder.Entity<Business>()
                .Property(p => p.SubscriptionStatus)
                .HasConversion<string>();

        }
    }
}
