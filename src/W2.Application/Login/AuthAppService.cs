using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Parlot.Fluent;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Identity;
using Volo.Abp.Security.Claims;
using W2.ExternalResources;
using W2.Identity;

namespace W2.Login
{
    public class AuthAppService : W2AppService, IAuthAppService
    {
        private readonly IdentityUserManager _userManager;
        private readonly IConfiguration _configuration;

        public AuthAppService(
            IdentityUserManager userManager,
            IConfiguration configuration
            )   
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<AuthUser> LoginAccount(AuthDto authDto)
        {
            var user = await _userManager.FindByNameAsync(authDto.userNameOrEmailAddress);

            if (user != null && !user.IsActive)
            {
                throw new UserFriendlyException("User is disabled!");
            }

            if (user != null && await _userManager.CheckPasswordAsync(user, authDto.password))
            {
                var token = GenerateJwtTokenForUser(user);
                return new AuthUser { Token = token };
            }

            throw new UserFriendlyException("Invalid username or password.");
        }

        private string GenerateJwtTokenForUser(Volo.Abp.Identity.IdentityUser user)
        {
            var roles = _userManager.GetRolesAsync(user).Result;
            DateTimeOffset now = (DateTimeOffset)DateTime.UtcNow;
            var claims = new List<Claim>
            {
                new Claim(AbpClaimTypes.UserId, user.Id.ToString()),
                new Claim(AbpClaimTypes.UserName, user.UserName),
                new Claim(AbpClaimTypes.Email, user.Email),
                new Claim(AbpClaimTypes.Name, user.Name),
                new Claim(AbpClaimTypes.Role, roles.FirstOrDefault()),
                new Claim(JwtRegisteredClaimNames.GivenName, user.Name),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Email),
                new Claim(JwtRegisteredClaimNames.AuthTime, now.ToUnixTimeSeconds().ToString()),
            };


            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(30),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
