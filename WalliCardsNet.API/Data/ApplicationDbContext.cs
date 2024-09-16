﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using WalliCardsNet.API.Data.Models;

namespace WalliCardsNet.API.Data
{
    // EF setup to use int Id's for ApplicationUser and IdentityRole

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }

        // DB sets
        public DbSet<Business> Businesses { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CardTemplate> CardTemplates { get; set; }
        public DbSet<Device> Devices { get; set; }

        // DB context configuration

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

        }

        // DB Model configuration

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityRole<int>>().HasData(
                new IdentityRole<int>
                {
                    Id = 1,
                    Name = Constants.Roles.Administrator,
                    NormalizedName = Constants.Roles.Administrator.ToUpper()
                },
                new IdentityRole<int>
                {
                    Id = 2,
                    Name = Constants.Roles.User,
                    NormalizedName = Constants.Roles.User.ToUpper()
                });
        }
    }
}
