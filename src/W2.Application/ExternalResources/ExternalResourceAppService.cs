using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2.Flows;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
using Volo.Abp.Caching;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.Security.Claims;
using W2.Identity;

namespace W2.ExternalResources
{
    public class ExternalResourceAppService : W2AppService, IExternalResourceAppService
    {
        private readonly IConfiguration _configuration;
        private readonly IdentityUserManager _userManager;
        private readonly SimpleGuidGenerator _simpleGuidGenerator;
        private readonly IDistributedCache<AllUserInfoCacheItem> _userInfoCache;
        private readonly IProjectClientApi _projectClient;
        private readonly ITimesheetClientApi _timesheetClient;
        //private readonly IHrmClientApi _hrmClient;
        private readonly List<OfficeInfo> listOfOffices = new List<OfficeInfo>
            {
                new OfficeInfo
                {
                    Code = "HN1",
                    DisplayName = "Hà Nội 1",
                    HeadOfOfficeEmail = "tung.nguyen@ncc.asia"
                },
                new OfficeInfo
                {
                    Code = "HN2",
                    DisplayName = "Hà Nội 2",
                    HeadOfOfficeEmail = "hieu.dohoang@ncc.asia"
                },
                new OfficeInfo
                {
                    Code = "HN3",
                    DisplayName = "Hà Nội 3",
                    HeadOfOfficeEmail = "quan.nguyenminh@ncc.asia"
                },
                new OfficeInfo
                {
                    Code = "ĐN",
                    DisplayName = "Đà Nẵng",
                    HeadOfOfficeEmail = "thien.dang@ncc.asia"
                },
                new OfficeInfo
                {
                    Code = "V",
                    DisplayName = "Vinh",
                    HeadOfOfficeEmail = "dai.trinhduc@ncc.asia"
                },
                new OfficeInfo
                {
                    Code = "SG1",
                    DisplayName = "Sài Gòn 1",
                    HeadOfOfficeEmail = "linh.nguyen@ncc.asia"
                },
                new OfficeInfo
                {
                    Code = "SG2",
                    DisplayName = "Sài Gòn 2",
                    HeadOfOfficeEmail = "linh.nguyen@ncc.asia"
                },
                new OfficeInfo
                {
                    Code = "QN",
                    DisplayName = "Quy Nhơn",
                    HeadOfOfficeEmail = "duy.nguyenxuan@ncc.asia"
                }
            };

        public ExternalResourceAppService(
            IDistributedCache<AllUserInfoCacheItem> userInfoCache,
            //IHrmClientApi hrmClient,
            IProjectClientApi projectClient,
            ITimesheetClientApi timesheetClient,
            IConfiguration configuration,
            IdentityUserManager userManager
            )
        {
            _userInfoCache = userInfoCache;
            _projectClient = projectClient;
            _timesheetClient = timesheetClient;
            //_hrmClient = hrmClient;
            _configuration = configuration;
            _userManager = userManager;
            _simpleGuidGenerator = SimpleGuidGenerator.Instance;
        }


        public async Task<List<UserInfoCacheItem>> GetAllUsersInfoAsync()
        {
            return await _userInfoCache.GetOrAddAsync(
                AllUserInfoCacheItem.CacheKey,
                async () => await GetAllUsersInfoFromApiAsync()
            );
        }

        public async Task<List<TimesheetProjectItem>> GetCurrentUserProjectsAsync()
        {
            var email = CurrentUser.Email;
            return await GetUserProjectsFromApiAsync(email);
        }

        public async Task RefreshAllUsersInfoAsync()
        {
            await _userInfoCache.RefreshAsync(AllUserInfoCacheItem.CacheKey);
        }

        public async Task<List<TimesheetProjectItem>> GetUserProjectsWithRolePMFromApiAsync()
        {
            var response = await _timesheetClient.GetUserProjectAsync(CurrentUser.Email);
            var projects = response.Result != null ? response.Result
                .Where(x => x.PM.Any(p => p.EmailAddress == CurrentUser.Email))
                .OrderBy(x => x.Name)
                .ThenBy(x => x.Code)
                .ToList() : new List<TimesheetProjectItem>();
            return projects;
        }

        public async Task<List<OfficeInfo>> GetListOfOfficeAsync()
        {
            return await Task.FromResult(this.listOfOffices);
        }

        private async Task<AllUserInfoCacheItem> GetAllUsersInfoFromApiAsync()
        {
            var response = await _projectClient.GetUsersAsync();
            var users = response.Result != null ? response.Result
                .OrderBy(x => x.Name)
                .ThenBy(x => x.Email)
                .ToList() : new List<UserInfoCacheItem>();

            return new AllUserInfoCacheItem(users);
        }

        public async Task<List<TimesheetProjectItem>> GetUserProjectsFromApiAsync(string email)
        {
            var response = await _timesheetClient.GetUserProjectAsync(email);
            var projects = response.Result != null ? response.Result
                .OrderBy(x => x.Name)
                .ThenBy(x => x.Code)
                .ToList() : new List<TimesheetProjectItem>();

            return projects;
        }

        public async Task<OfficeInfo> GetUserBranchInfoAsync(string email)
        {
            var response = await _timesheetClient.GetUserInfoByEmailAsync(email);
            var office = this.listOfOffices.FirstOrDefault(l => l.Code == response.Result.Branch);

            return office;
        }

        public async Task<ProjectProjectItem> GetCurrentUserWorkingProjectAsync()
        {
            var response = await _projectClient.GetUserProjectsAsync(CurrentUser.Email);
            return response.Result?.FirstOrDefault();
        }

        public async Task<TimesheetUserInfo> GetUserInfoByEmailAsync(string userEmail)
        {
            var response = await _timesheetClient.GetUserInfoByEmailAsync(userEmail);
            return response.Result;
        }

        // google login
        private async Task<GoogleJsonWebSignature.Payload> VerifyGoogleToken(ExternalAuthDto externalAuth)
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new List<string>() { _configuration["Authentication:Google:ClientId"] }
            };
            var payload = await GoogleJsonWebSignature.ValidateAsync(externalAuth.IdToken, settings);
            return payload;
        }

        [AllowAnonymous]
        public async Task<ExternalAuthUser> ExternalLogin(ExternalAuthDto externalAuth)
        {
            var payload = await this.VerifyGoogleToken(externalAuth);
            // verify @ncc.asia email
            if (!payload.Email.Contains("@ncc.asia"))
            {
                throw new UserFriendlyException("Invalid Email @ncc.asia.");
            }
            if (payload == null)
                throw new UserFriendlyException("Invalid External Authentication.");
            var info = new UserLoginInfo(externalAuth.Provider, payload.Subject, externalAuth.Provider);
            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (user != null && !user.IsActive)
            {
                throw new UserFriendlyException("User is disabled!");
            }
            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(payload.Email);
                if (user == null)
                {
                    user = new Volo.Abp.Identity.IdentityUser(_simpleGuidGenerator.Create(), payload.Email, payload.Email);
                    user.Name = payload.Name;
                    await _userManager.CreateAsync(user);
                    //prepare and send an email for the email confirmation
                    await _userManager.AddToRoleAsync(user, RoleNames.DefaultUser);
                    await _userManager.AddDefaultRolesAsync(user);
                }
                else
                {
                    await _userManager.AddLoginAsync(user, info);
                }
                await _userManager.UpdateAsync(user);
            }
            if (user == null)
                throw new UserFriendlyException("Invalid External Authentication.");
            //check for the Locked out account
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            var rolenames = await _userManager.GetRolesAsync(user);
            DateTimeOffset now = (DateTimeOffset)DateTime.UtcNow;  // using System;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(AbpClaimTypes.UserId, user.Id.ToString()),
                    new Claim(AbpClaimTypes.UserName, user.UserName),
                    new Claim(AbpClaimTypes.Email, user.Email),
                    new Claim(AbpClaimTypes.Name, user.Name),
                    new Claim(AbpClaimTypes.Role, rolenames.FirstOrDefault()),
                    new Claim(JwtRegisteredClaimNames.GivenName, user.Name),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.UniqueName, user.Email),
                    new Claim(JwtRegisteredClaimNames.AuthTime, now.ToUnixTimeSeconds().ToString()),
                 }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials
               (new SymmetricSecurityKey(key),
               SecurityAlgorithms.HmacSha512Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var stringToken = tokenHandler.WriteToken(token);
            return new ExternalAuthUser
            {
                Token = stringToken
            };
        }
    }
}
