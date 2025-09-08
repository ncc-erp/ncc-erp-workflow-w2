using DotLiquid;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using W2.Identity;
using W2.Mezon;
using W2.Settings;
using W2.Utils;
using W2.WorkflowDefinitions;
using static IdentityServer4.Models.IdentityResources;
using static Volo.Abp.Identity.Settings.IdentitySettingNames;

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
        private readonly IRepository<W2CustomIdentityUser, Guid> _userRepository;
        private readonly ILogger<ExternalResourceAppService> _logger;

        // private readonly IHrmClientApi _hrmClient;

        //private readonly IHrmClientApi _hrmClient;
        public ExternalResourceAppService(
            IDistributedCache<AllUserInfoCacheItem> userInfoCache,
            HttpClient httpClient,
            // IHrmClientApi hrmClient,
            IProjectClientApi projectClient,
            ITimesheetClientApi timesheetClient,
            IConfiguration configuration,
            IdentityUserManager userManager,
            ILogger<ExternalResourceAppService> logger,
            IRepository<W2CustomIdentityUser, Guid> userRepository,
            IRepository<W2Setting, Guid> settingRepository
            )
        {
            _userInfoCache = userInfoCache;
            _projectClient = projectClient;
            _timesheetClient = timesheetClient;
            _httpClient = httpClient;
            // _hrmClient = hrmClient;
            _configuration = configuration;
            _userManager = userManager;
            _simpleGuidGenerator = SimpleGuidGenerator.Instance;
            _userRepository = userRepository;
            _settingRepository = settingRepository;
            _logger = logger;
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
            if (!string.IsNullOrEmpty(email))
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
            settingValue.items.ForEach(item =>
            {
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
                    user = new W2CustomIdentityUser(_simpleGuidGenerator.Create(), payload.Email, payload.Email);
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
            // user 
            var query = await _userRepository.GetQueryableAsync();
            var userTemp = await query
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Where(u => u.Id == user.Id)
                .FirstOrDefaultAsync();
            ////check for the Locked out account
            //var issuer = _configuration["Jwt:Issuer"];
            //var audience = _configuration["Jwt:Audience"];
            //var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            //var rolenames = await _userManager.GetRolesAsync(user);
            //DateTimeOffset now = (DateTimeOffset)DateTime.UtcNow;  // using System;

            //var tokenDescriptor = new SecurityTokenDescriptor
            //{
            //    Subject = new ClaimsIdentity(new[]
            //    {
            //        new Claim(AbpClaimTypes.UserId, user.Id.ToString()),
            //        new Claim(AbpClaimTypes.UserName, user.UserName),
            //        new Claim(AbpClaimTypes.Email, user.Email),
            //        new Claim(AbpClaimTypes.Name, user.Name),
            //        new Claim(AbpClaimTypes.Role, rolenames.FirstOrDefault()),
            //        new Claim(JwtRegisteredClaimNames.GivenName, user.Name),
            //        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            //        new Claim(JwtRegisteredClaimNames.UniqueName, user.Email),
            //        new Claim(JwtRegisteredClaimNames.AuthTime, now.ToUnixTimeSeconds().ToString()),
            //     }),
            //    Expires = DateTime.UtcNow.AddMinutes(30),
            //    Issuer = issuer,
            //    Audience = audience,
            //    SigningCredentials = new SigningCredentials
            //   (new SymmetricSecurityKey(key),
            //   SecurityAlgorithms.HmacSha512Signature)
            //};

            //var tokenHandler = new JwtSecurityTokenHandler();
            //var token = tokenHandler.CreateToken(tokenDescriptor);
            //var stringToken = tokenHandler.WriteToken(token);
            var stringToken = Utils.JwtHelper.GenerateJwtTokenForUser(userTemp, _configuration);
            return new ExternalAuthUser
            {
                Token = stringToken
            };
        }

        public string MezonAuthUrl()
        {
            var host = $@"{_configuration["Authentication:Mezon:Domain"]}";
            var client_id = $@"{_configuration["Authentication:Mezon:ClientId"]}";
            var redirect_uri = $@"{_configuration["Authentication:Mezon:RedirectUri"]}";
            var state = GenerateBase64State();
            var auth_url = $@"{host}/oauth2/auth?client_id={client_id}&redirect_uri={redirect_uri}&response_type=code&scope=openid offline&state={state}";
            return auth_url;
        }

        public async Task<ExternalAuthUser> MezonLogin(MezonAuthDto mezonAuth)
        {
            try
            {
                var authConfig = _configuration.GetSection("Authentication:Mezon");
                var host = authConfig["Domain"];
                var tokenUrl = $"{host}/oauth2/token";
                var userInfoUrl = $"{host}/userinfo";

                var tokenResponse = await GetMezonAccessTokenAsync(tokenUrl, mezonAuth, authConfig);
                var mezonUserInfo = await GetMezonUserInfoAsync(userInfoUrl, tokenResponse.access_token);
                var mezonUserEmail = mezonUserInfo.sub;
                var query = await _userRepository.GetQueryableAsync();


                if (!mezonUserEmail.Contains("@ncc.asia"))
                {
                    var externalUser = await query
                        .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                        .Where(u => u.MezonUserId == mezonUserInfo.user_id)
                        .FirstOrDefaultAsync();

                    if (externalUser == null)
                    {
                        throw new UserFriendlyException("Fail to login with external user");
                    }

                    var externalJwtToken = Utils.JwtHelper.GenerateJwtTokenForUser(externalUser, _configuration);

                    return new ExternalAuthUser { Token = externalJwtToken };
                }


                var existedUser = await _userManager.FindByEmailAsync(mezonUserEmail);
                if (existedUser == null)
                {
                    // Create new user if not exist
                    var response = await _timesheetClient.GetUserInfoByEmailAsync(mezonUserEmail);

                    if (response == null)
                    {
                        throw new UserFriendlyException("User info not found from Timesheet");
                    }

                    var userHrmName = response.Result.FullName;
                    existedUser = new W2CustomIdentityUser(_simpleGuidGenerator.Create(), mezonUserEmail, mezonUserEmail);
                    existedUser.Name = Helper.ConvertVietnameseToUnsign(userHrmName);

                    await _userManager.CreateAsync(existedUser);
                    await _userManager.AddToRoleAsync(existedUser, RoleNames.DefaultUser);
                    await _userManager.AddDefaultRolesAsync(existedUser);
                }

                var finalUser = await query
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .Where(u => u.MezonUserId == mezonUserInfo.user_id || u.Email == existedUser.Email)
                    .FirstOrDefaultAsync();

                var jwtToken = Utils.JwtHelper.GenerateJwtTokenForUser(finalUser, _configuration);

                return new ExternalAuthUser { Token = jwtToken };
            }
            catch (UserFriendlyException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw new UserFriendlyException("Invalid Mezon Authentication.");
            }
        }

        private async Task<MezonOauthTokenResponse> GetMezonAccessTokenAsync(
            string tokenUrl, MezonAuthDto mezonAuth, IConfigurationSection authConfig)
        {
            var bodyData = new Dictionary<string, string>
            {
                { "code", mezonAuth.code },
                { "state", mezonAuth.state },
                { "grant_type", "authorization_code" },
                { "redirect_uri", authConfig["RedirectUri"] },
                { "scope", mezonAuth.scope },
                { "client_id", authConfig["ClientId"] },
                { "client_secret", authConfig["ClientSecret"] }
            };

            var response = await _httpClient.PostAsync(tokenUrl, new FormUrlEncodedContent(bodyData));
            if (!response.IsSuccessStatusCode)
                throw new UserFriendlyException("Invalid Mezon Authentication.");

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<MezonOauthTokenResponse>(responseContent);
        }

        private async Task<MezonAuthUserDto> GetMezonUserInfoAsync(string userInfoUrl, string accessToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, userInfoUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                throw new UserFriendlyException("Failed to get Mezon user info");

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<MezonAuthUserDto>(content);
        }

        private string GenerateBase64State()
        {
            byte[] randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }
    }
}
