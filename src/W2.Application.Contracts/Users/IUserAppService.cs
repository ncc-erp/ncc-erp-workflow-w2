using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using W2.Identity;
using W2.Roles;
using Volo.Abp.Identity;

namespace W2.Users
{
    public interface IUserAppService : IApplicationService
    {
        Task<PagedResultDto<UserDto>> GetListAsync(ListUsersInput input);
        Task<List<IdentityRoleDto>> GetUserRolesAsync(Guid userId);
        Task<List<PermissionDetailDto>> GetUserPermissionsAsync(Guid userId);
        Task<UserDetailDto> UpdateUserAsync(Guid userId, UpdateUserInput input);
    }
}
