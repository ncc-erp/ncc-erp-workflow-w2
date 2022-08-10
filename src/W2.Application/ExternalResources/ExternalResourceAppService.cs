using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Caching;

namespace W2.ExternalResources
{
    public class ExternalResourceAppService : W2AppService, IExternalResourceAppService
    {
        private readonly IDistributedCache<AllUserInfoCacheItem> _userInfoCache;
        private readonly IHrmClientApi _hrmClient;

        public ExternalResourceAppService(IDistributedCache<AllUserInfoCacheItem> userInfoCache,
            IHrmClientApi hrmClient)
        {
            _userInfoCache = userInfoCache;
            _hrmClient = hrmClient;
        }


        public async Task<List<UserInfoCacheItem>> GetAllUsersInfoAsync()
        {
            return await _userInfoCache.GetOrAddAsync(
                AllUserInfoCacheItem.CacheKey,
                async () => await GetAllUsersInfoFromApiAsync()
            );
        }

        public async Task<List<ProjectItem>> GetCurrentUserProjectsAsync()
        {
            var email = CurrentUser.Email;
            return await GetUserProjectsFromApiAsync(email);
        }

        public async Task RefreshAllUsersInfoAsync()
        {
            await _userInfoCache.RefreshAsync(AllUserInfoCacheItem.CacheKey);
        }

        public async Task<List<ProjectItem>> GetUserProjectsWithRolePMFromApiAsync()
        {
            var response = await _hrmClient.GetUserProjectAsync(CurrentUser.Email);
            var projects = response.Result != null ? response.Result
                .Where(x => x.PM.EmailAddress == CurrentUser.Email)
                .OrderBy(x => x.Name)
                .ThenBy(x => x.Code)
                .ToList() : new List<ProjectItem>();
            return projects;
        }

        private async Task<AllUserInfoCacheItem> GetAllUsersInfoFromApiAsync()
        {
            var response = await _hrmClient.GetUsersAsync();
            var users = response.Result != null ? response.Result
                .OrderBy(x => x.Name)
                .ThenBy(x => x.Email)
                .ToList() : new List<UserInfoCacheItem>();

            return new AllUserInfoCacheItem(users);
        }

        private async Task<List<ProjectItem>> GetUserProjectsFromApiAsync(string email)
        {
            var response = await _hrmClient.GetUserProjectAsync(email);
            var projects = response.Result != null ? response.Result
                .OrderBy(x => x.Name)
                .ThenBy(x => x.Code)
                .ToList() : new List<ProjectItem>();

            return projects;
        }
    }
}
