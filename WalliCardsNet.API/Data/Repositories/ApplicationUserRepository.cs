using Microsoft.EntityFrameworkCore;
using WalliCardsNet.API.Data.Interfaces;
using WalliCardsNet.API.Models;
using Microsoft.AspNetCore.Identity;
using WalliCardsNet.ClassLibrary.ApplicationUser;

namespace WalliCardsNet.API.Data.Repositories
{
    public class ApplicationUserRepository : IApplicationUser
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public ApplicationUserRepository(ApplicationDbContext applicationDbContext, UserManager<ApplicationUser> userManager)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
        } 
        public async Task<ApplicationUser> GetByActivationTokenAsync(Guid activationToken)
        {
            var token = await _applicationDbContext.ActivationTokens
                    .Include(t => t.ApplicationUser) 
                    .FirstOrDefaultAsync(t => t.Id == activationToken);

            if (token != null)
            {
                return token.ApplicationUser;
            }
            return null;
        }

        public async Task SetPasswordAsync(ApplicationUserDTO userDTO)
        {
            var user = await _userManager.FindByIdAsync(userDTO.Id);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            var passwordResult = await _userManager.HasPasswordAsync(user);
            IdentityResult result;

            if (passwordResult)
            {
                result = await _userManager.RemovePasswordAsync(user); 
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }

            result = await _userManager.AddPasswordAsync(user, userDTO.Password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
