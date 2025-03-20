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
using W2.Utils;
using Volo.Abp.Guids;

namespace W2.Login
{
    public class AuthAppService : W2AppService, IAuthAppService
    {
        private readonly IdentityUserManager _userManager;
        private readonly IRepository<W2CustomIdentityUser, Guid> _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly SimpleGuidGenerator _simpleGuidGenerator;

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
            _simpleGuidGenerator = SimpleGuidGenerator.Instance;
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

            var token = JwtHelper.GenerateJwtTokenForUser(user, _configuration);
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

        public async Task<AuthUser> LoginMezonByHash(AuthMezonByHashDto authMezonByHashDto)
        {
            if (string.IsNullOrEmpty(authMezonByHashDto.hashKey) || string.IsNullOrEmpty(authMezonByHashDto.dataCheck))
            {
                throw new ArgumentNullException(nameof(authMezonByHashDto.hashKey));
            }

            var appToken = _configuration["Authentication:Mezon:AppToken"]
                ?? throw new ArgumentNullException("Mezon AppToken configuration is missing.");

            byte[] secretKey = Hasher.HMAC_SHA256(Encoding.UTF8.GetBytes(appToken), Encoding.UTF8.GetBytes("WebAppData"));
            var hashedData = Hasher.HEX(Hasher.HMAC_SHA256(secretKey, Encoding.UTF8.GetBytes(authMezonByHashDto.dataCheck)));

            if (!authMezonByHashDto.hashKey.Equals(hashedData))
            {
                throw new UserFriendlyException("Authentication failed - Invalid hash key");
            }

            var query = await _userRepository.GetQueryableAsync();
            var user = await query
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)                    
                .Where(u => u.UserName == authMezonByHashDto.userName ||
                            u.Email == authMezonByHashDto.userEmail)
                .FirstOrDefaultAsync();

            if (user != null && !user.IsActive)
            {
                throw new UserFriendlyException("User is disabled!");
            }
            if (user == null)
            {
                    user = new W2CustomIdentityUser(_simpleGuidGenerator.Create(), authMezonByHashDto.userEmail, authMezonByHashDto.userEmail);
                    user.Name = authMezonByHashDto.userName;

                    await _userManager.CreateAsync(user);
                    await _userManager.AddToRoleAsync(user, RoleNames.DefaultUser);
                    await _userManager.AddDefaultRolesAsync(user);

                    await _userManager.UpdateAsync(user);
            }

            var queryTem = await _userRepository.GetQueryableAsync();
            var userTemp = await queryTem
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Where(u => u.Id == user.Id)
                .FirstOrDefaultAsync();

            var token = JwtHelper.GenerateJwtTokenForUser(userTemp, _configuration);
            return new AuthUser { Token = token };
        }
    }
}
