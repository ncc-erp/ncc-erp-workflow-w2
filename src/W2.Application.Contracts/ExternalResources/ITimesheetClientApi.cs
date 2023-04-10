using Refit;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace W2.ExternalResources
{
    public interface ITimesheetClientApi : IApplicationService
    {
        [Get("/api/services/app/Public/GetPMsOfUser")]
        Task<AbpResponse<TimesheetProjectItem>> GetUserProjectAsync([AliasAs("email")] string email);

        [Get("/api/services/app/HRMv2/GetUserInfoByEmail")]
        Task<AbpResponse<TimesheetUserInfo>> GetUserInfoByEmailAsync([AliasAs("email")] string email);
    }
}
