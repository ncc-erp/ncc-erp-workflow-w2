using Refit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace W2.ExternalResources
{
    public interface IHrmClient
    {
        [Get("/hrm/users")]
        Task<List<UserInfoCacheItem>> GetUsersAsync();
    }
}
