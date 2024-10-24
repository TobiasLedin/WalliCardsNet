using Blazored.LocalStorage;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using WalliCardsNet.Client.Models;
using WalliCardsNet.Client.Services;

namespace WalliCardsNet.Client.Handlers
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;
        private readonly ClientAuthService _authService;
        private bool _refreshInProgress;
        private string? _newToken;
        private DateTime? _tokenExpiry;
        private readonly TimeSpan _refreshThreshold = TimeSpan.FromMinutes(5);
        private readonly int _awaitRefreshCycles = 3;
        private readonly int _awaitRefreshCycleDelay = 200;

        public AuthHeaderHandler(ILocalStorageService localStorage, ClientAuthService authService)
        {
            _localStorage = localStorage;
            _authService = authService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _localStorage.GetItemAsync<string>("accessToken");

            if (!string.IsNullOrEmpty(token))
            {
                if (_tokenExpiry == null)
                {
                    UpdateTokenExpiry(token);
                }

                if (TokenAboutToExpire())
                {
                    var refreshResult = await RefreshTokenAsync(cancellationToken);

                    if (refreshResult.Success)
                    {
                        token = refreshResult.Token;
                    }
                }

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshResult = await RefreshTokenAsync(cancellationToken);

                if (refreshResult.Success)
                {
                    var newRequest = await CloneHttpRequestMessageAsync(request);

                    newRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshResult.Token);

                    response = await base.SendAsync(newRequest, cancellationToken);
                }
            }

            return response;
        }

        private async Task<RefreshResult> RefreshTokenAsync(CancellationToken cancellationToken)
        {
            if (_refreshInProgress)
            {
                for (int i = 0; i < _awaitRefreshCycles; i++)
                {
                    await Task.Delay(_awaitRefreshCycleDelay, cancellationToken);

                    if (!_refreshInProgress && !string.IsNullOrEmpty(_newToken))
                    {
                        return new RefreshResult { Success = true, Token = _newToken };
                    }
                }

                return new RefreshResult { Success = false };
            }

            try
            {
                _refreshInProgress = true;
                _newToken = null;

                var refreshResult = await _authService.RefreshAccessTokenAsync();

                if (refreshResult.Success)
                {
                    _newToken = refreshResult.Token;
                    UpdateTokenExpiry(refreshResult.Token!);

                    return refreshResult;
                }
                else
                {
                    return new RefreshResult { Success = false };
                }

            }
            finally
            {
                _refreshInProgress = false;
            }
        }

        private bool TokenAboutToExpire()
        {
            if (!_tokenExpiry.HasValue)
            {
                return false;
            }

            return DateTime.UtcNow.Add(_refreshThreshold) >= _tokenExpiry.Value;
        }

        private void UpdateTokenExpiry(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadToken(token);

                if (jwtToken?.ValidTo != null)
                {
                    _tokenExpiry = jwtToken.ValidTo.ToUniversalTime();
                }
            }
            catch 
            {
                _tokenExpiry = null;
            }
        }

        private async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request)
        {
            var newRequest = new HttpRequestMessage(request.Method, request.RequestUri);

            if (request.Content != null)
            {
                var content = await request.Content.ReadAsStringAsync();
                newRequest.Content = new StringContent(content, Encoding.UTF8, request.Content.Headers.ContentType?.MediaType!);
            }

            foreach (var header in request.Headers)
            {
                newRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return newRequest;
        }

    }
}
