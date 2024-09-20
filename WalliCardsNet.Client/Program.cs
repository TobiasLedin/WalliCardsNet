using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WalliCardsNet.Client.Handlers;

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
            builder.Services.AddHttpClient("wallicardAPI", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7204");
            })
            .AddHttpMessageHandler<AuthTokenHandler>();

            //builder.Services.AddAuthorizationCore

            await builder.Build().RunAsync();
        }
    }
}
