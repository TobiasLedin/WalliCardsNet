using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Data.Models;
using WalliCardsNet.API.Data.Repositories;
using WalliCardsNet.ClassLibrary;

namespace WalliCardsNet.API.Services
{
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

        /// <summary>
        /// Register a new ApplicationUser with role User.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        public async Task<RegisterResponseDTO> RegisterEmployeeAsync(string userName, string email, string password)
        {
            if (email != null || password != null)
            {
                //var business = _businessRepository.GetByIdAsync(1);

                var user = new ApplicationUser
                {
                    //Business = "123",
                    UserName = userName,
                    NormalizedUserName = userName.ToUpper(),
                    Email = email,
                    NormalizedEmail = email!.ToUpper()
                };

                var userNameValidation = await ValidateUserNameAsync(user);
                var passwordValidation = await ValidatePasswordAsync(user, password);

                if (userNameValidation.Succeeded && passwordValidation.Succeeded)
                {
                    try
                    {
                        var createUserResult = await _userManager.CreateAsync(user);

                        if (createUserResult.Succeeded)
                        {
                            await _userManager.AddPasswordAsync(user, password);      // Set password in separate endpoint
                            await _userManager.SetEmailAsync(user, email);
                            await _userManager.AddToRoleAsync(user, Constants.Roles.Manager);
                            await _userManager.SetLockoutEnabledAsync(user, false);  // Remove ?

                            return new RegisterResponseDTO(true, null);
                        }

                        return new RegisterResponseDTO(false, null);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }

            return new RegisterResponseDTO(false, "Email/Password not provided");
        }

        /// <summary>
        /// Client login method
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns>LoginResultDTO containing access token, if login is successful.</returns>
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

        #region Tokens
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

        #region Support methods and classes

        // ClaimsIdentity generator
        private async Task<List<Claim>> GenerateClaimsAsync(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim("security_stamp", user.SecurityStamp!),
                //new Claim("business_id", user.Business!.Id.ToString())                  //TODO: Add BusinessId
            };

            var roles = await _userManager.GetRolesAsync(user);

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }

        public class RegisterResult
        {
            public bool RegisterSuccess { get; set; }
            public string? Email { get; set; }
            public string? Details { get; set; }
        }

        public class LoginResult
        {
            public bool LoginSuccess { get; set; }

            public string? Token { get; set; }

            public string? Details { get; set; }
        }

        #endregion
    }
}
