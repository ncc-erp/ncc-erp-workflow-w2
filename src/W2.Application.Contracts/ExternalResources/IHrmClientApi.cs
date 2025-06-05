using Refit;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace W2.ExternalResources
{
    public interface IHrmClientApi : IApplicationService
    {
        [Get("/api/services/app/Public/GetEmployeeByEmail")]
        Task<AbpResponse<UserBranchInfo>> GetUserBranchInfoAsync([AliasAs("email")] string email);

        [Get("/api/services/app/Public/GetAllEmployee")]
        Task<AbpResponse<HrmEmployeeInfo>> GetAllEmployee();
    }
}
