using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Data.Repositories;
using WalliCardsNet.API.Models;
using WalliCardsNet.ClassLibrary.Login;
using WalliCardsNet.ClassLibrary.Register;

namespace WalliCardsNet.API.Services
{

    // Author: Tobias
    // Service to manage all login/authentication and account creation related tasks.
    public class APIAuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBusiness _businessRepository;
        private readonly IConfiguration _config;

        public APIAuthService(
            UserManager<ApplicationUser> userManager,
            IBusiness businessRepository,
            IConfiguration config)
        {
            _userManager = userManager;
            _businessRepository = businessRepository;
            _config = config;
        }

        // Create new ApplicationUser account.
        public async Task<RegisterResponseDTO> CreateUserAccountAsync(Guid businessId, string role, string userName, string email)
        {
            if (role != null && userName != null && email != null)
            {
                var business = await _businessRepository.GetByIdAsync(businessId);

                if (business != null)
                {
                    var user = new ApplicationUser
                    {
                        Business = business,
                        BusinessId = businessId,
                        UserName = userName,
                        NormalizedUserName = userName.ToUpper(),
                        Email = email,
                        NormalizedEmail = email!.ToUpper()
                    };

                    var userNameValidation = await ValidateUserNameAsync(user);

                    if (!userNameValidation.Succeeded)
                    {
                        return new RegisterResponseDTO(false, "Username validation failed", null);
                    }

                    try
                    {
                        var createUserResult = await _userManager.CreateAsync(user);

                        if (createUserResult.Succeeded)
                        {
                            await _userManager.SetEmailAsync(user, email);
                            await _userManager.AddToRoleAsync(user, role);

                            return new RegisterResponseDTO(true, null, user.Id);
                        }
                    }
                    catch (Exception)
                    {
                        return new RegisterResponseDTO(false, "Failed to create and assign user properties", null);
                    }
                }
            }

            return new RegisterResponseDTO(false, "Required arguments not provided", null);
        }

        // Client login method.
        // Checks if user account exists, whether the account has been locked out and verifies the password.
        public async Task<LoginResponseDTO> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                var lockoutActive = await _userManager.IsLockedOutAsync(user);

                if (lockoutActive)
                {
                    var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);

                    return new LoginResponseDTO(false, null, $"Maximum allowed login attempts exceeded. Lockout enabled until {lockoutEnd:g}");
                }

                var passwordIsValid = await _userManager.CheckPasswordAsync(user, password);

                if (passwordIsValid)
                {
                    var token = await GenerateTokenAsync(user);

                    return new LoginResponseDTO(true, token, null);
                }

                await _userManager.AccessFailedAsync(user);
            }

            return new LoginResponseDTO(false, null, "Email/Password incorrect");
        }

        //TODO: Extract to separate helper classes?
        #region Support methods

        // JWT generator
        private async Task<string> GenerateTokenAsync(ApplicationUser user)
        {
            var privateKey = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT-PRIVATE-KEY")
                                ?? throw new ArgumentNullException("JWT-PRIVATE-KEY is not set"));

            var jwtIssuer = _config["JwtSettings:Issuer"];
            var jwtAudience = _config["JwtSettings:Audience"];
            var jwtExpire = _config.GetValue<double>("JwtSettings:ExpireMinutes");
            var jwtCredentials = new SigningCredentials(new SymmetricSecurityKey(privateKey), SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: await GenerateClaimsAsync(user),
                expires: DateTime.UtcNow.AddMinutes(jwtExpire),
                signingCredentials: jwtCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ClaimsIdentity generator
        private async Task<List<Claim>> GenerateClaimsAsync(ApplicationUser user)
        {
            var business = await _businessRepository.GetByIdAsync(user.BusinessId);
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
        #endregion

        #region Validators

        // Username validator
        public async Task<IdentityResult> ValidateUserNameAsync(ApplicationUser user)
        {
            foreach (var validator in _userManager.UserValidators)
            {
                var result = await validator.ValidateAsync(_userManager, user);

                if (!result.Succeeded)
                {
                    return result;
                }
            }

            return IdentityResult.Success;
        }

        // Password validator
        public async Task<IdentityResult> ValidatePasswordAsync(ApplicationUser user, string password)
        {
            foreach (var validator in _userManager.PasswordValidators)
            {
                var result = await validator.ValidateAsync(_userManager, user, password);

                if (!result.Succeeded)
                {
                    return result;
                }
            }

            return IdentityResult.Success;
        }

        #endregion
    }
}
