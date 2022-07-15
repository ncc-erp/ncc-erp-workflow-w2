using System.Collections.Generic;

namespace W2.ExternalResources
{
    public class AllUserInfoCacheItem : List<UserInfoCacheItem>
    {
        public const string CacheKey = "AllUserInfo";

        public AllUserInfoCacheItem()
        {
        }

        public AllUserInfoCacheItem(IEnumerable<UserInfoCacheItem> items) : base(items)
        {
        }
    }
}
