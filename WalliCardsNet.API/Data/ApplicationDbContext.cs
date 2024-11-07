using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WalliCardsNet.API.Models;
using WalliCardsNet.ClassLibrary.BusinessProfile;
using WalliCardsNet.ClassLibrary.BusinessProfile.Models;

namespace WalliCardsNet.API.Data
{
    
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        // DB sets
        public DbSet<Business> Businesses { get; set; }
        public DbSet<BusinessProfile> Profiles { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CardTemplate> CardTemplates { get; set; } //TODO: Obsolete
        public DbSet<ActivationToken> ActivationTokens { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<JoinForm> JoinForms { get; set; }
        public DbSet<GooglePassTemplate> GooglePassTemplates { get; set; }
        public DbSet<GooglePass> GooglePasses { get; set; }


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

            // Model relationsships
            builder.Entity<Business>()
                .HasMany(b => b.Customers)
                .WithOne()
                .HasForeignKey(c => c.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<BusinessProfile>()
                .HasOne<Business>()
                .WithMany(b => b.Profiles)
                .HasForeignKey(bp => bp.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<BusinessProfile>()
                .HasOne(bp => bp.GoogleTemplate)
                .WithOne()
                .HasForeignKey<GooglePassTemplate>(gpt => gpt.BusinessProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<BusinessProfile>()
                .HasOne(bp => bp.JoinForm)
                .WithOne()
                .HasForeignKey<JoinForm>(jf => jf.BusinessProfileId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
