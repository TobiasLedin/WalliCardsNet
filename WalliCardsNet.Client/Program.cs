using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WalliCardsNet.Client.Handlers;
using WalliCardsNet.Client.Services;

namespace WalliCardsNet.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Services.AddBlazoredLocalStorage();

            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7204")
                
            });

            // TEST (HttpClientFactory)
            builder.Services.AddHttpClient("WalliCardsApi", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7204");
            })
            .AddHttpMessageHandler<AuthMessageHandler>();

            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthenticationStateProvider, AuthStateProvider>();

            builder.Services.AddScoped<WalliCardsApiService>();
            builder.Services.AddScoped<ClientAuthService>();
            builder.Services.AddScoped<AuthMessageHandler>();


            await builder.Build().RunAsync();
        }
    }
}
