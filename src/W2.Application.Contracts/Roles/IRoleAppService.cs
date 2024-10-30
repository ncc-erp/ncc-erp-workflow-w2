using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;

namespace W2.Roles
{
    public interface IRoleAppService : IApplicationService
    {
        Task<ListResultDto<RoleDto>> GetRolesAsync();
        Task<RoleDetailDto> GetRoleDetailAsync(Guid id);
        Task<RoleDetailDto> CreateRoleAsync(CreateRoleInput input);
        Task<RoleDetailDto> UpdateRoleAsync(Guid roleId, UpdateRoleInput input);
        Task<List<PermissionDetailDto>> GetPermissionsAsync();
    }
}
