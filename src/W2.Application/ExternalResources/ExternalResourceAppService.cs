using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Caching;

namespace W2.ExternalResources
{
    public class ExternalResourceAppService : W2AppService, IExternalResourceAppService
    {
        private readonly IDistributedCache<AllUserInfoCacheItem> _userInfoCache;
        private readonly IProjectClientApi _projectClient;
        private readonly ITimesheetClientApi _timesheetClient;

        public ExternalResourceAppService(
            IDistributedCache<AllUserInfoCacheItem> userInfoCache,
            IProjectClientApi projectClient,
            ITimesheetClientApi timesheetClient)
        {
            _userInfoCache = userInfoCache;
            _projectClient = projectClient;
            _timesheetClient = timesheetClient;
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

        private async Task<AllUserInfoCacheItem> GetAllUsersInfoFromApiAsync()
        {
            var response = await _projectClient.GetUsersAsync();
            var users = response.Result != null ? response.Result
                .OrderBy(x => x.Name)
                .ThenBy(x => x.Email)
                .ToList() : new List<UserInfoCacheItem>();

            return new AllUserInfoCacheItem(users);
        }

        private async Task<List<TimesheetProjectItem>> GetUserProjectsFromApiAsync(string email)
        {
            var response = await _timesheetClient.GetUserProjectAsync(email);
            var projects = response.Result != null ? response.Result
                .OrderBy(x => x.Name)
                .ThenBy(x => x.Code)
                .ToList() : new List<TimesheetProjectItem>();

            return projects;
        }
    }
}
