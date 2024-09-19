
using DotNetEnv;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WalliCardsNet.API.Data;
using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Data.Repositories;
using WalliCardsNet.API.Data.Models;
using Microsoft.AspNetCore.Authentication;
using WalliCardsNet.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace WalliCardsNet.API
{
    public class Program
    {
        public static void Main(string[] args)
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

            // Mail service
            builder.Services.AddTransient<IMailService, MailService>();

            // FormData service
            builder.Services.AddTransient<FormDataService>();

            // Identity
            // Service registration and setup
            builder.Services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";

            })
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
            });

            builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT-PRIVATE-KEY")!)),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = Environment.GetEnvironmentVariable("JWT-VALID-AUDIENCE"),   // Needs configuring!
                    ValidIssuer = Environment.GetEnvironmentVariable("JWT-VALID-ISSUER")        // Needs configuring!
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
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();


            app.Run();
        }
    }
}
