
using DotNetEnv;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WalliCardsNet.API.Data;
using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Data.Repositories;
using Microsoft.AspNetCore.Authentication;
using WalliCardsNet.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WalliCardsNet.API.Data.Seeders;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using WalliCardsNet.API.Models;
using Stripe;

namespace WalliCardsNet.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();

            // Swagger
            builder.Services.AddSwaggerGen();

            // DotNetEnv
            // Load environment variables from .env file
            Env.Load();

            // DB connection string retrieval from environment variables
            var connectionString = Environment.GetEnvironmentVariable("CONNECTION-STRING");

            // Stripe configuration
            StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("STRIPE-SECRET-KEY");

            // EntityFramework
            // Service registration
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Repositories
            builder.Services.AddTransient<IBusiness, BusinessRepository>();
            builder.Services.AddTransient<ICardTemplate, CardTemplateRepository>();
            builder.Services.AddTransient<ICustomer, CustomerRepository>();
            builder.Services.AddTransient<IDevice, DeviceRepository>();
            builder.Services.AddTransient<IFormData, FormDataRepository>();
            builder.Services.AddTransient<IActivationToken, ActivationTokenRepository>();

            // Mail service
            builder.Services.AddTransient<IMailService, MailService>();

            // FormData service
            builder.Services.AddTransient<FormDataService>();

            // Identity
            // Service registration
            builder.Services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // Identity
            // Settings
            builder.Services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequiredLength = 8;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;

                // User settings
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";

                // SignIn settings
                options.SignIn.RequireConfirmedAccount = false;
            });

            builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT-PRIVATE-KEY")!)),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["JwtSettings:Audience"],
                    ValidIssuer = builder.Configuration["JwtSettings:Issuer"]
                };
            });

            builder.Services.AddAuthorization();

            // Custom Authentication service
            builder.Services.AddScoped<IAuthService, APIAuthService>();


            //CORS
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins("https://localhost:7102")
                           .AllowAnyHeader()
                           .AllowAnyMethod();
                });
            });

            var app = builder.Build();
            app.UseCors();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();

                using (var scope = app.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    var appDbContext = services.GetRequiredService<ApplicationDbContext>();
                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

                    await TestDataSeeder.SeedAsync(appDbContext, userManager);
                }
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();


            app.Run();
        }
    }
}
