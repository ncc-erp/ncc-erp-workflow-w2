using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace W2.ExternalResources
{
    public interface IExternalResourceAppService
    {
        Task<List<UserInfoCacheItem>> GetAllUsersInfoAsync();
        Task<List<ProjectItem>> GetCurrentUserProjectsAsync();
        Task RefreshAllUsersInfoAsync();
    }
}
