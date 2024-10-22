using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;

namespace W2.Roles
{
    public interface IRoleAppService : IApplicationService
    {
        //Task<ListResultDto<IdentityRoleDto>> AllAsync();
        Task<IdentityRoleDto> CreateRole(CreateRoleInput input);
        //Task<IdentityRoleDto> ApplyPermissionAsync(CreateRoleInput input);
    }
}
