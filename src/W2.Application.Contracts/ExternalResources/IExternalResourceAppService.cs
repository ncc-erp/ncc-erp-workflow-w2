using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace W2.ExternalResources
{
    public interface IExternalResourceAppService
    {
        Task<List<UserInfoCacheItem>> GetAllUsersInfoAsync();
        Task<List<TimesheetProjectItem>> GetCurrentUserProjectsAsync();
        Task RefreshAllUsersInfoAsync();
        Task<List<TimesheetProjectItem>> GetUserProjectsWithRolePMFromApiAsync();
        Task<List<OfficeInfo>> GetListOfOfficeAsync();
        Task<OfficeInfo> GetUserBranchInfoAsync(string userEmail);
        Task<ProjectProjectItem> GetCurrentUserWorkingProjectAsync();
    }
}
