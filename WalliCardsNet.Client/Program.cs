using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Net;
using WalliCardsNet.Client.Handlers;
using WalliCardsNet.Client.Services;
using WalliCardsNet.ClassLibrary.Services;

namespace WalliCardsNet.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddHttpClient("WalliCardsApi", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7204");
            })
            .AddHttpMessageHandler<AuthHeaderHandler>();

            builder.Services.AddHttpClient("AuthClient", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7204/api/auth/");
            })
            .AddHttpMessageHandler(() => new IncludeCredentialsHandler());

            builder.Services.AddTransient<IClassLibraryBusinessProfilesService, ClassLibraryBusinessProfilesService>();

            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddBlazorBootstrap();

            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(x => x.GetRequiredService<AuthStateProvider>());
            builder.Services.AddScoped<WalliCardsApiService>();
            builder.Services.AddScoped<ClientAuthService>();
            builder.Services.AddScoped<AuthHeaderHandler>();

            await builder.Build().RunAsync();
        }
    }
}
