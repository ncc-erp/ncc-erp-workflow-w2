using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2.Flows;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.Security.Claims;
using W2.Identity;
using W2.Settings;
using W2.WorkflowDefinitions;

namespace W2.ExternalResources
{
    public class ExternalResourceAppService : W2AppService, IExternalResourceAppService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly IdentityUserManager _userManager;
        private readonly SimpleGuidGenerator _simpleGuidGenerator;
        private readonly IDistributedCache<AllUserInfoCacheItem> _userInfoCache;
        private readonly IProjectClientApi _projectClient;
        private readonly ITimesheetClientApi _timesheetClient;
        private readonly IRepository<W2Setting, Guid> _settingRepository;
        //private readonly IHrmClientApi _hrmClient;
        public ExternalResourceAppService(
            IDistributedCache<AllUserInfoCacheItem> userInfoCache,
            HttpClient httpClient,
            //IHrmClientApi hrmClient,
            IProjectClientApi projectClient,
            ITimesheetClientApi timesheetClient,
            IConfiguration configuration,
            IdentityUserManager userManager,
            IRepository<W2Setting, Guid> settingRepository
            )
        {
            _userInfoCache = userInfoCache;
            _projectClient = projectClient;
            _timesheetClient = timesheetClient;
            _httpClient = httpClient;
            //_hrmClient = hrmClient;
            _configuration = configuration;
            _userManager = userManager;
            _simpleGuidGenerator = SimpleGuidGenerator.Instance;
            _settingRepository = settingRepository;
        }


        public async Task<List<UserInfoCacheItem>> GetAllUsersInfoAsync()
        {
            return await _userInfoCache.GetOrAddAsync(
                AllUserInfoCacheItem.CacheKey,
                async () => await GetAllUsersInfoFromApiAsync()
            );
        }

        public async Task<List<ReleaseContent>> GetGithubReleaseContentAsync()
        {
            // URLs cần gọi
            var urls = new[]
            {
                "https://api.github.com/repos/ncc-erp/ncc-erp-workflow-w2-ui/releases",
                "https://api.github.com/repos/ncc-erp/ncc-erp-workflow-w2/releases"
            };

            var allReleases = new List<ReleaseContent>();

            foreach (var url in urls)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent", "HttpClient");
                request.Headers.Remove("RequestVerificationToken");

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Failed to get releases from {url}: {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();

                // Deserialize và thêm vào danh sách chung
                var releases = JsonConvert.DeserializeObject<List<ReleaseContent>>(responseContent);
                if (releases != null)
                {
                    allReleases.AddRange(releases);
                }
            }

            return allReleases;
        }


        public async Task<List<TimesheetProjectItem>> GetCurrentUserProjectsAsync(string email)
        {
            var userEmail = CurrentUser.Email;
            if(!string.IsNullOrEmpty(email))
            {
                userEmail = email;
            }

            return await GetUserProjectsFromApiAsync(userEmail);
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
            return await GetListOfOffice();
        }

        private async Task<List<OfficeInfo>> GetListOfOffice()
        {
            var setting = await _settingRepository.FirstOrDefaultAsync(setting => setting.Code == SettingCodeEnum.DIRECTOR);
            var settingValue = setting.ValueObject;
            List<OfficeInfo> officeInfoList = new List<OfficeInfo>();
            settingValue.items.ForEach(item => {
                officeInfoList.Add(new OfficeInfo
                {
                    Code = item.code,
                    DisplayName = item.name,
                    HeadOfOfficeEmail = item.email,
                });
            });
            return officeInfoList;
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
            var officeList = await GetListOfOffice();
            var office = officeList.FirstOrDefault(l => l.Code == response.Result.Branch);

            return office;
        }

        public async Task<ProjectProjectItem> GetCurrentUserWorkingProjectAsync(string email)
        {
            var userEmail = CurrentUser.Email;
            if (!string.IsNullOrEmpty(email))
            {
                userEmail = email;
            }

            var response = await _projectClient.GetUserProjectsAsync(userEmail);
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

        public Task<List<InputDefinitionTypeItemDto>> GetWorkflowInputDefinitionPropertyTypes()
        {
            var enumValues = Enum.GetValues(typeof(WorkflowInputDefinitionProperyType))
                                 .Cast<WorkflowInputDefinitionProperyType>()
                                 .Select(e => new InputDefinitionTypeItemDto
                                 {
                                     Value = e.ToString(),
                                     Label = e.ToString()
                                 })
                                 .ToList();

            return Task.FromResult(enumValues);
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
