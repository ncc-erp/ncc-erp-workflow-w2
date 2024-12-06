using IdentityModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Security.Claims;
using W2.Identity;
using W2.Permissions;

namespace W2.Login
{
    public class AuthAppService : W2AppService, IAuthAppService
    {
        private readonly IdentityUserManager _userManager;
        private readonly IRepository<W2CustomIdentityUser, Guid> _userRepository;
        private readonly IConfiguration _configuration;

        public AuthAppService(
            IdentityUserManager userManager,
            IRepository<W2CustomIdentityUser, Guid> userRepository,
            IConfiguration configuration
        )
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<AuthUser> LoginAccount(AuthDto authDto)
        {
            var query = await _userRepository.GetQueryableAsync();
            var user = await query
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)                    
                .Where(u => u.UserName == authDto.userNameOrEmailAddress ||
                            u.Email == authDto.userNameOrEmailAddress)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                throw new UserFriendlyException("Invalid username or password.");
            }

            if (!user.IsActive)
            {
                throw new UserFriendlyException("User is disabled!");
            }

            var isCorrectPassword = await _userManager.CheckPasswordAsync(user, authDto.password);
            if (!isCorrectPassword)
            {
                throw new UserFriendlyException("Invalid username or password.");
            }

            var token = Utils.JwtHelper.GenerateJwtTokenForUser(user, _configuration);
            return new AuthUser { Token = token };
        }

    }
}
