using IdentityModel;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Security.Claims;
using W2.Identity;
using W2.Permissions;

namespace W2.Utils
{
    static class JwtHelper
    {

        public static string GenerateJwtTokenForUser(W2CustomIdentityUser user, Microsoft.Extensions.Configuration.IConfiguration _configuration)
        {
            var roleNames = user.UserRoles
                .Select(ur => ur.Role.Name)
                .ToArray();
            var rolePermissions = user.UserRoles
                .SelectMany(ur => ur.Role.PermissionDtos)
                .ToList();

            var rolePermissionCodes = W2Permission.GetPermissionCodes(rolePermissions);
            var customPermissionCodes = W2Permission.GetPermissionCodes(user.CustomPermissionDtos);
            var allPermissionCodes = rolePermissionCodes
                .Union(customPermissionCodes)
                .OrderBy(x => x)
                .ToList();

            DateTimeOffset now = (DateTimeOffset)DateTime.UtcNow;

            var claims = new List<Claim>
            {
                new Claim(AbpClaimTypes.UserId, user.Id.ToString()),
                new Claim(AbpClaimTypes.UserName, user.UserName),
                new Claim(AbpClaimTypes.Email, user.Email),
                new Claim(AbpClaimTypes.Name, user.Name),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Email),
                new Claim(JwtRegisteredClaimNames.AuthTime, now.ToUnixTimeSeconds().ToString()),
            };
            claims.AddRange(roleNames.Select(
                role => new Claim(JwtClaimTypes.Role, role))
            );
            claims.AddRange(allPermissionCodes.Select(
                permission => new Claim("permissions", permission))
            );

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
