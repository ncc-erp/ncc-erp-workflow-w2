using Refit;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace W2.ExternalResources
{
    public interface IProjectClientApi: IApplicationService
    {
        [Get("/api/services/app/Public/GetAllUser")]
        Task<AbpResponse<UserInfoCacheItem>> GetUsersAsync();

        [Get("/api/services/app/Public/GetPMOfUser")]
        Task<AbpResponse<ProjectItem>> GetUserProjectAsync([AliasAs("email")] string email);
    }
}
