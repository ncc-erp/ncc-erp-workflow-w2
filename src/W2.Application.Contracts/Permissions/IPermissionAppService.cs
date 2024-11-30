using System;
using System.Threading.Tasks;
using W2.Permissions;
using W2.Authorization.Attributes;
using W2.Roles;

namespace W2.Permissions
{
    public interface IPermissionAppService
    {
        Task<PermissionDetailDto> CreatePermissionAsync(CreatePermissionInput input);

        Task<PermissionDetailDto> UpdatePermissionAsync(Guid id, UpdatePermissionInput input);

        Task DeletePermissionAsync(Guid id);
    }
}
