using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace W2.ExternalResources
{
    public interface IExternalResourceAppService
    {
        Task<List<UserInfoCacheItem>> GetAllUsersInfoAsync();
        Task<List<TimesheetProjectItem>> GetCurrentUserProjectsAsync(string? email = null);
        Task<List<TimesheetProjectItem>> GetUserProjectsFromApiAsync(string email);
        Task RefreshAllUsersInfoAsync();
        Task<List<TimesheetProjectItem>> GetUserProjectsWithRolePMFromApiAsync();
        Task<List<OfficeInfo>> GetListOfOfficeAsync();
        Task<OfficeInfo> GetUserBranchInfoAsync(string userEmail);
        Task<List<InputDefinitionTypeItemDto>> GetWorkflowInputDefinitionPropertyTypes();
        Task<ProjectProjectItem> GetCurrentUserWorkingProjectAsync(string? email = null);
        Task<TimesheetUserInfo> GetUserInfoByEmailAsync(string userEmail);
        Task<ExternalAuthUser> ExternalLogin(ExternalAuthDto externalAuth);
        string MezonAuthUrl();
        Task<ExternalAuthUser> MezonLogin(MezonAuthDto mezonAuth);
    }
}
