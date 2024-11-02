using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBusiness _businessRepository;
        private readonly IGoogleService _googleService;
        private readonly ITokenService _tokenService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IBusiness businessRepository,
            IGoogleService googleService,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _businessRepository = businessRepository;
            _googleService = googleService;
            _tokenService = tokenService;
        }

        // Create new ApplicationUser account.
        public async Task<RegisterResponseDTO> CreateUserAccountAsync(Guid businessId, string role, string userName, string email)
        {
            if (role != null && userName != null && email != null)
            {
                var user = new ApplicationUser
                {
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

            return new RegisterResponseDTO(false, "Required arguments not provided", null);
        }

        // Client login method.
        // Checks if user account exists, whether the account has been locked out and verifies the password.
        public async Task<AuthResult> AuthenticateUserAsync(string email, string password)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);

                if (user != null)
                {
                    var lockoutActive = await _userManager.IsLockedOutAsync(user);

                    if (lockoutActive)
                    {
                        var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);

                        return new AuthResult { Success = false, Details = $"Maximum allowed login attempts exceeded. Lockout enabled until {lockoutEnd:g}" };
                    }

                    var passwordIsValid = await _userManager.CheckPasswordAsync(user, password);

                    if (passwordIsValid)
                    {
                        var business = await _businessRepository.GetByIdAsync(user.BusinessId);

                        return new AuthResult { Success = true, User = user, Business = business, Details = "Login successful!" };
                    }

                    await _userManager.AccessFailedAsync(user);
                }

                return new AuthResult { Success = false, Details = "Email/Password incorrect" };
            }
            catch (Exception ex)
            {
                return new AuthResult { Success = false, Details = ex.Message };
            }
        }

        public async Task<AuthResult> LoginWithGoogleAsync(string code)
        {
            var tokenData = await _googleService.ExchangeCodeForTokensAsync(code, "https://localhost:7102/auth/google/login/");
            var idToken = tokenData["id_token"]?.ToString();
            if (idToken == null)
            {
                return new AuthResult { Success = false, Details = "Failed to retrieve ID token from Google." };
            }
            var (googleUserId, googleEmail) = _googleService.DecodeIdToken(idToken);

            var user = await _userManager.FindByEmailAsync(googleEmail);
            if (user == null)
            {
                return new AuthResult { Success = false, Details = "No user found with this Google email." };
            }

            var business = await _businessRepository.GetByIdAsync(user.BusinessId);

            var claims = await _tokenService.GenerateClaimsAsync(user, business);
            var accessToken = _tokenService.GenerateAccessToken(claims);
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(Guid.Parse(user.Id));

            return new AuthResult { Success = true, AccessToken = accessToken, RefreshToken = refreshToken, Details = "Login successful!" };
        }

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
