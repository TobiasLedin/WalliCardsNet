using Google.Apis.Walletobjects.v1.Data;
using System.Text.Json;
using WalliCardsNet.API.Models;

namespace WalliCardsNet.API.Services.GoogleServices.GoogleAuth
{
    public interface IGoogleAuth
    {
        Task<Dictionary<string, object>> ExchangeCodeForTokensAsync(string code, string redirectUri);
        (string googleUserId, string googleEmail) DecodeIdToken(string idToken);
        Task<bool> LinkGoogleAccountAsync(ApplicationUser user, string googleUserId);

    }
}
