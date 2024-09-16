using Microsoft.AspNetCore.Identity;
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
                            await _userManager.AddPasswordAsync(user, password);
                            await _userManager.SetEmailAsync(user, email);
                            await _userManager.AddToRoleAsync(user, Constants.Roles.User);

                            return new RegisterResult { RegisterSuccess = true, Email = user.Email };
                        }

                        return new RegisterResult { RegisterSuccess = false, Details = createUserResult.Errors.ToString() };

                    }
                    catch (Exception ex)
                    {

                    }

                }
                else
                {
                    //foreach (var error in userNameValidation.Errors)
                    //{
                    //    results.Add(error)
                    //}

                }
            }

            return new RegisterResult { RegisterSuccess = false, Details = "Email/Password not provided" };
        }

        public async Task<LoginResult> Login(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                var passwordIsValid = await _userManager.CheckPasswordAsync(user, password);

                if (passwordIsValid)
                {
                    return new LoginResult { LoginSuccess = true };
                }
            }

            return new LoginResult { LoginSuccess = false };
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
        #region Support classes

        // Support classes
        public class RegisterResult
        {
            public bool RegisterSuccess { get; set; }
            public string? Email { get; set; }
            public string? Details { get; set; }
        }

        public class LoginResult
        {
            public bool LoginSuccess { get; set; }
        }

        #endregion
    }
}
