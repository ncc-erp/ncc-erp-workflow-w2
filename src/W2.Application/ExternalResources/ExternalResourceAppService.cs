using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Caching;

namespace W2.ExternalResources
{
    public class ExternalResourceAppService : W2AppService, IExternalResourceAppService
    {
        private readonly IDistributedCache<AllUserInfoCacheItem> _userInfoCache;
        private readonly IHrmClient _hrmClient;

        public ExternalResourceAppService(IDistributedCache<AllUserInfoCacheItem> userInfoCache, 
            IHrmClient hrmClient)
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

        public async Task RefreshAllUsersInfoAsync()
        {
            await _userInfoCache.RefreshAsync(AllUserInfoCacheItem.CacheKey);
        }

        private async Task<AllUserInfoCacheItem> GetAllUsersInfoFromApiAsync()
        {
            var users = (await _hrmClient.GetUsersAsync())
                .OrderBy(x => x.Name)
                .ThenBy(x => x.Email)
                .ToList();

            return new AllUserInfoCacheItem(users);
        }
    }
}
