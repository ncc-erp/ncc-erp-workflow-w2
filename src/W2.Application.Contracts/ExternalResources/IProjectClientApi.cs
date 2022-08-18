using Refit;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace W2.ExternalResources
{
    public interface IProjectClientApi : IApplicationService
    {
        [Get("/api/services/app/Public/GetAllUser")]
        Task<AbpResponse<UserInfoCacheItem>> GetUsersAsync();

        [Get("/api/services/app/Public/GetPMOfUser")]
        Task<AbpResponse<ProjectProjectItem>> GetUserProjectsAsync([AliasAs("email")] string email);
    }
}
