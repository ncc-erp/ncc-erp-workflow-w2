using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Users;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using W2.Permissions;
using System.Linq;
using Volo.Abp.Uow;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Microsoft.EntityFrameworkCore;

namespace W2.Roles
{
    [Authorize]
    [Route("api/app/roles")]
    public class RoleAppService : W2AppService, IRoleAppService
    {
        private readonly IRepository<IdentityRole, Guid> _roleRepository;
        private readonly IIdentityRoleRepository _identityRoleRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IRepository<W2Permission, Guid> _permissionRepository;
        private readonly IRepository<W2PermissionRole, Guid> _permissionRoleRepository;

        public RoleAppService(
            IRepository<IdentityRole, Guid> roleRepository,
            IIdentityRoleRepository identityRoleRepository,
            ICurrentUser currentUser,
            IRepository<W2Permission, Guid> permissionRepository,
            IRepository<W2PermissionRole, Guid> permissionRoleRepository)
        {
            _roleRepository = roleRepository;
            _identityRoleRepository = identityRoleRepository;
            _currentUser = currentUser;
            _permissionRepository = permissionRepository;
            _permissionRoleRepository = permissionRoleRepository;
        }

        [HttpGet]
        public async Task<ListResultDto<IdentityRoleDto>> GetRolesAsync()
        {
            var roles = await _roleRepository.GetListAsync();
            return new ListResultDto<IdentityRoleDto>(
                ObjectMapper.Map<List<IdentityRole>, List<IdentityRoleDto>>(roles)
            );
        }

        [HttpGet("{id}")]
        public async Task<RoleDetailDto> GetRoleDetailsAsync(Guid id)
        {
            var role = await _roleRepository.GetAsync(id)
                ?? throw new UserFriendlyException($"Role with id {id} not found");

            var query = await _permissionRoleRepository.GetQueryableAsync();
            var rolePermissions = await query
                .Include(pr => pr.Permission)
                .Where(pr => pr.RoleId == id)
                .Select(pr => pr.Permission)
                .ToListAsync();
            var permissionHierarchy = BuildPermissionHierarchy(rolePermissions);

            var roleDetailsDto = ObjectMapper.Map<IdentityRole, RoleDetailDto>(role);
            roleDetailsDto.Permissions = permissionHierarchy;

            return roleDetailsDto;
        }

        [HttpPost]
        public async Task<RoleDetailDto> CreateRoleAsync(CreateRoleInput input)
        {
            if (input.PermissionCodes == null || !input.PermissionCodes.Any())
            {
                throw new UserFriendlyException("At least one permission are required");
            }

            using var unitOfWork = UnitOfWorkManager.Begin();

            try
            {
                var permissions = await _permissionRepository.GetListAsync(
                    p => input.PermissionCodes.Contains(p.Code)
                );
                ValidatePermissions(permissions, input.PermissionCodes);

                var role = new IdentityRole(
                    GuidGenerator.Create(),
                    input.Name,
                    CurrentTenant.Id
                );
                await _roleRepository.InsertAsync(role);
                
                var rolePermissionMappings = permissions.Select(
                    permission => new W2PermissionRole(permission.Id, role.Id, CurrentTenant.Id)
                );
                foreach (var mapping in rolePermissionMappings)
                {
                    await _permissionRoleRepository.InsertAsync(mapping);
                }
                
                await unitOfWork.CompleteAsync();

                var roleDetailDto = ObjectMapper.Map<IdentityRole, RoleDetailDto>(role);
                roleDetailDto.Permissions = BuildPermissionHierarchy(permissions);

                return roleDetailDto;
            }
            catch (Exception)
            {
                await unitOfWork.RollbackAsync();
                throw;
            }
        }

        [HttpPut("{roleId}")]
        public async Task<RoleDetailDto> UpdateRoleAsync(Guid roleId, UpdateRoleInput input)
        {
            if (input.PermissionCodes == null || !input.PermissionCodes.Any())
            {
                throw new UserFriendlyException("At least one permission are required");
            }

            using var unitOfWork = UnitOfWorkManager.Begin();

            try
            {
                var role = await _roleRepository.GetAsync(roleId)
                    ?? throw new UserFriendlyException($"Role with id {roleId} not found");

                if (!string.IsNullOrEmpty(input.Name) && role.Name != input.Name)
                {
                    role.ChangeName(input.Name);
                    await _roleRepository.UpdateAsync(role);
                }

                var newPermissions = await _permissionRepository
                    .GetListAsync(p => input.PermissionCodes.Contains(p.Code));
                ValidatePermissions(newPermissions, input.PermissionCodes);

                var existingPermissionRoles = await _permissionRoleRepository
                        .GetListAsync(pr => pr.RoleId == roleId);

                var newPermissionIds = newPermissions.Select(p => p.Id).ToList();
                var existingPermissionIds = existingPermissionRoles.Select(pr => pr.PermissionId).ToList();

                var (permissionsToRemove, permissionsToAdd) = GetPermissionChanges(
                    existingPermissionIds,
                    newPermissionIds
                );

                await RemovePermissionsFromRoleAsync(roleId, permissionsToRemove);
                await AddPermissionsToRoleAsync(roleId, permissionsToAdd);

                var roleDetailDto = ObjectMapper.Map<IdentityRole, RoleDetailDto>(role);
                roleDetailDto.Permissions = BuildPermissionHierarchy(newPermissions);

                return roleDetailDto;
            }
            catch (Exception)
            {
                await unitOfWork.RollbackAsync();
                throw;
            }
        }

        //[HttpPost("permissions")]
        //public async Task<IActionResult> CreatePermissionAsync(List<CreatePermissionInput> inputs)
        //{
        //    var createdPermissions = new List<W2Permission>();

        //    foreach (var input in inputs)
        //    {
        //        var permission = new W2Permission
        //        {
        //            Name = input.Name,
        //            Code = input.Code,
        //            ParentId = input.ParentId,
        //        };

        //        await _permissionRepository.InsertAsync(permission);
        //        createdPermissions.Add(permission);
        //    }

        //    return new OkObjectResult(createdPermissions);
        //}

        [HttpGet("permissions")]
        public async Task<List<PermissionDetailDto>> GetPermissionsAsync()
        {
            var allPermissions = await _permissionRepository.GetListAsync();
            return BuildPermissionHierarchy(allPermissions);
        }

        private (List<Guid> ToRemove, List<Guid> ToAdd) GetPermissionChanges(
            List<Guid> existingPermissionIds,
            List<Guid> newPermissionIds)
        {
            var toRemove = existingPermissionIds.Except(newPermissionIds).ToList();
            var toAdd = newPermissionIds.Except(existingPermissionIds).ToList();
            return (toRemove, toAdd);
        }

        private void ValidatePermissions(List<W2Permission> permissions, List<string> requestedCodes)
        {
            if (permissions.Count != requestedCodes.Count)
            {
                throw new UserFriendlyException("Some requested permissions do not exist");
            }

            var parentPermissions = permissions.Count(p => p.ParentId == null);
            var childPermissions = permissions.Count(p => p.ParentId != null);

            if (parentPermissions == 0)
            {
                throw new UserFriendlyException("At least one permission is required");
            }
            if (childPermissions == 0)
            {
                throw new UserFriendlyException("At least one permission is required");
            }

            var parentIds = permissions
                .Where(p => p.ParentId == null)
                .Select(p => p.Id)
                .ToHashSet();

            var orphanedChildren = permissions
                .Where(p => p.ParentId != null && !parentIds.Contains(p.ParentId.Value))
                .ToList();

            if (orphanedChildren.Any())
            {
                var orphanedCodes = string.Join(", ", orphanedChildren.Select(p => p.Code));
                throw new UserFriendlyException(
                    $"Child permissions must belong to selected parent permissions. Orphaned permissions: {orphanedCodes}"
                );
            }
        }

        private async Task AddPermissionsToRoleAsync(Guid roleId, List<Guid> permissionIds)
        {
            if (permissionIds.Any())
            {
                var newMappings = permissionIds.Select(
                    permissionId => new W2PermissionRole(permissionId, roleId, CurrentTenant.Id)
                );

                foreach (var mapping in newMappings)
                {
                    await _permissionRoleRepository.InsertAsync(mapping);
                }
            }
        }

        private async Task RemovePermissionsFromRoleAsync(Guid roleId, List<Guid> permissionIds)
        {
            if (permissionIds.Any())
            {
                await _permissionRoleRepository.DeleteAsync(
                    pr => pr.RoleId == roleId && permissionIds.Contains(pr.PermissionId)
                );
            }
        }

        private List<PermissionDetailDto> BuildPermissionHierarchy(List<W2Permission> permissions)
        {
            return permissions
                .Where(p => p.ParentId == null)
                .Select(parent =>
                {
                    var parentDto = ObjectMapper.Map<W2Permission, PermissionDetailDto>(parent);
                    parentDto.Children = permissions
                        .Where(c => c.ParentId == parent.Id)
                        .Select(child => ObjectMapper.Map<W2Permission, PermissionDto>(child))
                        .ToList();
                    return parentDto;
                })
                .ToList();
        }
    }
}
