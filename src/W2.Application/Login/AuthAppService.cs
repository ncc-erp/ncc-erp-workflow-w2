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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Security.Claims;
using W2.Authorization.Attributes;
using W2.Identity;
using W2.Permissions;

namespace W2.Login
{
    public class AuthAppService : W2AppService, IAuthAppService
    {
        private readonly IdentityUserManager _userManager;
        private readonly IRepository<W2CustomIdentityUser, Guid> _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthAppService(
            IdentityUserManager userManager,
            IRepository<W2CustomIdentityUser, Guid> userRepository,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
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
        
        [HttpGet]
        //[Authorize]
        public new UserInfo CurrentUser()
        {
            var claims = _httpContextAccessor.HttpContext?.User.Claims;

            var sub = claims
                .Where(x => x.Type == JwtRegisteredClaimNames.Sub)
                .Select(x => x.Value)
                .ToArray();
            
            var name = claims
                .First(x => x.Type == AbpClaimTypes.UserName)
                .Value;
            
            var email = claims
                .First(x => x.Type == AbpClaimTypes.Email)
                .Value;
            
            var given_name = claims
                .First(x => x.Type == AbpClaimTypes.Name)
                .Value;
            
            var role = claims
                .First(x => x.Type == JwtClaimTypes.Role)
                .Value;
            
            var permissions = claims
                .Where(x => x.Type == "permissions")
                .Select(x => x.Value)
                .ToArray();


            return new UserInfo
            {
                sub = sub,
                name = name,
                email = email,
                given_name = given_name,
                role = role,
                permissions = permissions
            };
        }

    }
}
