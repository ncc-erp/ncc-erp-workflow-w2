using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;
using W2.Permissions;
using W2.Authorization.Attributes;
using Volo.Abp.PermissionManagement;
using W2.Constants;
using W2.Roles;

namespace W2.Permissions
{
    [Route("api/app/permissions")]
    public class PermissionAppService : W2AppService, IPermissionAppService
    {
        private readonly IRepository<W2Permission, Guid> _permissionRepository;

        public PermissionAppService(IRepository<W2Permission, Guid> permissionRepository)
        {
            _permissionRepository = permissionRepository;
        }

        [HttpPost]
        [RequirePermission(W2ApiPermissions.CreatePermissions)]
        public async Task<PermissionDetailDto> CreatePermissionAsync(CreatePermissionInput input)
        {
            var permission = new W2Permission(
                input.Name,
                input.Code,
                input.ParentId,
                CurrentTenant.Id
            );

            await _permissionRepository.InsertAsync(permission);
            return ObjectMapper.Map<W2Permission, PermissionDetailDto>(permission);
        }

        // API để cập nhật quyền
        [HttpPut("{id}")]
        [RequirePermission(W2ApiPermissions.UpdatePermissions)]
        public async Task<PermissionDetailDto> UpdatePermissionAsync(Guid id, UpdatePermissionInput input)
        {
            var permission = await _permissionRepository.GetAsync(id);

            permission.SetName(input.Name);
            permission.SetCode(input.Code);
            permission.SetParentId(input.ParentId);


            await _permissionRepository.UpdateAsync(permission);
            return ObjectMapper.Map<W2Permission, PermissionDetailDto>(permission);
        }

        // API để xóa quyền
        [HttpDelete("{id}")]
        [RequirePermission(W2ApiPermissions.UpdatePermissions)]
        public async Task DeletePermissionAsync(Guid id)
        {
            var permission = await _permissionRepository.GetAsync(id);
            await _permissionRepository.DeleteAsync(permission);
        }
    }
}
