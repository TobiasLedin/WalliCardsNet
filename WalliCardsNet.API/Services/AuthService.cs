using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WalliCardsNet.API.Data.Models;

namespace WalliCardsNet.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// Register a new ApplicationUser with role User.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        public async Task<RegisterResult> Register(string userName, string email, string password)
        {
            if (email != null || password != null)
            {
                List<string> results = new();

                var user = new ApplicationUser
                {
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
                            //await _userManager.AddPasswordAsync(user, password);      // Set password in separate endpoint
                            await _userManager.SetEmailAsync(user, email);
                            await _userManager.AddToRoleAsync(user, Constants.Roles.Manager);
                            await _userManager.SetLockoutEnabledAsync(user, false);

                            return new RegisterResult { RegisterSuccess = true, Email = user.Email };
                        }

                        return new RegisterResult { RegisterSuccess = false, Details = createUserResult.Errors.ToString() }; //TODO: Fix error messages

                    }
                    catch (Exception ex)
                    {

                    }
                }
            }

            return new RegisterResult { RegisterSuccess = false, Details = "Email/Password not provided" };
        }

        public async Task<LoginResult> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                var lockoutActive = await _userManager.IsLockedOutAsync(user);

                if (lockoutActive)
                {
                    var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                    
                    return new LoginResult { LoginSuccess = false, Details = $"Maximum allowed login attempts exceeded. Lockout enabled until {lockoutEnd:g}" };
                }

                var passwordIsValid = await _userManager.CheckPasswordAsync(user, password);

                if (passwordIsValid)
                {
                    return new LoginResult { LoginSuccess = true, Token = await GenerateTokenAsync(user) };
                }

                await _userManager.AccessFailedAsync(user);
            }

            return new LoginResult { LoginSuccess = false, Details = "Email and/or Password incorrect!" };
        }

        #region Tokens
        private async Task<string> GenerateTokenAsync(ApplicationUser user)
        {
            var privateKey = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT-PRIVATE-KEY")!);

            var tokenHandler = new JwtSecurityTokenHandler();

            var credentials = new SigningCredentials(new SymmetricSecurityKey(privateKey), SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                SigningCredentials = credentials,
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(Environment.GetEnvironmentVariable("JWT-EXPIRE-TIME"))),
                Subject = await GenerateClaimsAsync(user)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
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
        private async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim("security_stamp", user.SecurityStamp!),
            };

            var roles = await _userManager.GetRolesAsync(user);

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return new ClaimsIdentity(claims);
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
