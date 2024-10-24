using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace WalliCardsNet.Client.Handlers
{
    public class IncludeCredentialsHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            return base.SendAsync(request, cancellationToken);
        }

    }
}
