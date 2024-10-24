using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WalliCardsNet.API.Data.Repositories;
using WalliCardsNet.API.Models;

namespace WalliCardsNet.API.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRefreshToken _tokenRepository;

        public TokenService(IConfiguration config, UserManager<ApplicationUser> userManager, IRefreshToken tokenRepository)
        {
            _config = config;
            _userManager = userManager;
            _tokenRepository = tokenRepository;
        }

        public async Task<bool> RefreshTokenValidationAsync(string refreshToken)
        {
            var storedRefreshToken = await _tokenRepository.GetAsync(refreshToken);
            if (storedRefreshToken == null)
            {
                return false;
            }

            return storedRefreshToken.Expiry >= DateTime.UtcNow;
        }

        public async Task DeleteRefreshTokenAsync(string refreshToken)
        {
            await _tokenRepository.RemoveAsync(refreshToken);
            await _tokenRepository.RemoveExpiredAsync();
        }

        // JWT access token generator
        public string GenerateAccessToken(IEnumerable<Claim> claims)
        {
            var privateKey = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT-PRIVATE-KEY")
                                ?? throw new ArgumentNullException("JWT-PRIVATE-KEY is not set"));

            var jwtIssuer = _config["JwtSettings:Issuer"];
            var jwtAudience = _config["JwtSettings:Audience"];
            var jwtExpire = _config.GetValue<double>("JwtSettings:AccessTokenExpireMinutes");
            var jwtCredentials = new SigningCredentials(new SymmetricSecurityKey(privateKey), SecurityAlgorithms.HmacSha256);

            var filteredClaims = claims.Where(claim => claim.Type != JwtRegisteredClaimNames.Aud).ToList();

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: filteredClaims,
                expires: DateTime.UtcNow.AddMinutes(jwtExpire),
                signingCredentials: jwtCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> GenerateRefreshTokenAsync(Guid userId)
        {
            var randomNumber = new byte[32];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            };

            var token = Convert.ToBase64String(randomNumber);
            var expiry = _config.GetValue<double>("JwtSettings:RefreshTokenExpireHours");

            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = token,
                Expiry = DateTime.UtcNow.AddHours(expiry)
            };

            await _tokenRepository.AddAsync(refreshToken);

            return refreshToken.Token;
        }

        // ClaimsIdentity generator
        public async Task<List<Claim>> GenerateClaimsAsync(ApplicationUser user, Business business)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim("security-stamp", user.SecurityStamp!),
                new Claim("business-id", user.BusinessId.ToString()!),
                new Claim("business-token", business.UrlToken )
            };

            var logins = await _userManager.GetLoginsAsync(user);
            var googleLogin = logins.FirstOrDefault(login => login.LoginProvider == "Google");
            if (googleLogin != null)
            {
                claims.Add(new Claim("google-id", googleLogin.ProviderKey));
            }

            var roles = await _userManager.GetRolesAsync(user);

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken)
        {
            var privateKey = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT-PRIVATE-KEY")
                                ?? throw new ArgumentNullException("JWT-PRIVATE-KEY is not set"));

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _config["JwtSettings:Issuer"],
                ValidAudience = _config["JwtSettings:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(privateKey)
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(accessToken.Trim('"'), tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }


    }
}
