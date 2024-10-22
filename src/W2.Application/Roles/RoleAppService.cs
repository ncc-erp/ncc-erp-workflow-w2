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
        public async Task<ListResultDto<IdentityRoleDto>> GetAllRoles()
        {
            var roles = await _roleRepository.GetListAsync();
            return new ListResultDto<IdentityRoleDto>(
                ObjectMapper.Map<List<IdentityRole>, List<IdentityRoleDto>>(roles)
            );
        }

        [HttpGet("roles/{id}")]
        public async Task<RoleDetailDto> GetRoleById(Guid id)
        {
            // Get role
            var role = await _roleRepository.GetAsync(id);
            if (role == null)
            {
                throw new UserFriendlyException($"Role with id {id} not found");
            }

            // Get all permissions of role
            var query = await _permissionRoleRepository.GetQueryableAsync();
            var permissions = await query
                .Include(pr => pr.Permission)
                .Where(pr => pr.RoleId == id)
                .Select(pr => pr.Permission)
                .ToListAsync();

            var permissionIds = permissions.Select(p => p.Id).ToList();

            // Get all permissions and organize them
            //var allPermissions = await _permissionRepository.GetListAsync();

            // Get parent permissions that role has access to
            var parentPermissions = permissions
                .Where(p => p.ParentId == null)
                .Select(parent => new PermissionDetailDto
                {
                    Id = parent.Id,
                    Name = parent.Name,
                    Code = parent.Code,
                    CreationTime = parent.CreationTime,
                    Children = permissions
                        .Where(c => c.ParentId == parent.Id)
                        .Select(child => new PermissionDto
                        {
                            Id = child.Id,
                            Name = child.Name,
                            Code = child.Code,
                            CreationTime = child.CreationTime,
                        })
                        .ToList(),
                })
                .ToList();

            // Map to DTO
            var roleDto = ObjectMapper.Map<IdentityRole, RoleDetailDto>(role);
            roleDto.Permissions = parentPermissions;

            return roleDto;
        }

        [HttpPost]
        public async Task<IdentityRoleDto> CreateRole(CreateRoleInput input)
        {
            using (var uow = UnitOfWorkManager.Begin())
            {
                try
                {
                    // Create new role
                    var role = new IdentityRole(
                        GuidGenerator.Create(),
                        input.Name
                    );
                    await _roleRepository.InsertAsync(role);

                    // Get all permissions by names
                    var permissions = await _permissionRepository.GetListAsync(
                        p => input.PermissionNames.Contains(p.Name)
                    );

                    // Validate if all requested permissions exist
                    if (permissions.Count != input.PermissionNames.Count)
                    {
                        throw new UserFriendlyException("Some permissions do not exist");
                    }

                    // Create permission-role records
                    var permissionRoles = permissions.Select(permission => new W2PermissionRole
                    {
                        PermissionId = permission.Id,
                        RoleId = role.Id,
                    }).ToList();

                    // Insert permission-role records
                    foreach (var permissionRole in permissionRoles)
                    {
                        await _permissionRoleRepository.InsertAsync(permissionRole);
                    }

                    await uow.CompleteAsync();

                    return ObjectMapper.Map<IdentityRole, IdentityRoleDto>(role);
                }
                catch (Exception ex)
                {
                    await uow.RollbackAsync();
                    throw ex;
                }
            }
        }

        [HttpPut("{roleId}")]
        public async Task<IdentityRoleDto> UpdateRole(Guid roleId, UpdateRoleInput input)
        {
            using (var uow = UnitOfWorkManager.Begin())
            {
                try
                {
                    // Get and update role
                    var role = await _roleRepository.GetAsync(roleId);
                    if (role == null)
                    {
                        throw new UserFriendlyException($"Role with id {roleId} not found");
                    }

                    // Update role name if changed
                    if (!string.IsNullOrEmpty(input.Name) && role.Name != input.Name)
                    {
                        role.ChangeName(input.Name);
                        await _roleRepository.UpdateAsync(role);
                    }

                    // Handle permissions if provided
                    if (input.PermissionNames != null && input.PermissionNames.Any())
                    {
                        // Get existing permission-role mappings
                        var existingPermissionRoles = await _permissionRoleRepository
                            .GetListAsync(pr => pr.RoleId == roleId);

                        // Get all requested permissions
                        var newPermissions = await _permissionRepository
                            .GetListAsync(p => input.PermissionNames.Contains(p.Name));

                        // Validate if all requested permissions exist
                        if (newPermissions.Count != input.PermissionNames.Count)
                        {
                            throw new UserFriendlyException("Some permissions do not exist");
                        }

                        // Get permissions to remove and to add
                        var newPermissionIds = newPermissions.Select(p => p.Id).ToList();
                        var existingPermissionIds = existingPermissionRoles.Select(pr => pr.PermissionId).ToList();

                        var permissionIdsToRemove = existingPermissionIds
                            .Except(newPermissionIds)
                            .ToList();

                        var permissionIdsToAdd = newPermissionIds
                            .Except(existingPermissionIds)
                            .ToList();

                        // Remove old permissions
                        if (permissionIdsToRemove.Any())
                        {
                            await _permissionRoleRepository.DeleteAsync(
                                pr => pr.RoleId == roleId && permissionIdsToRemove.Contains(pr.PermissionId)
                            );
                        }

                        // Add new permissions
                        if (permissionIdsToAdd.Any())
                        {
                            var permissionRolesToAdd = permissionIdsToAdd.Select(permissionId => new W2PermissionRole
                            {
                                PermissionId = permissionId,
                                RoleId = roleId,
                                TenantId = CurrentTenant.Id
                            });

                            foreach (var permissionRole in permissionRolesToAdd)
                            {
                                await _permissionRoleRepository.InsertAsync(permissionRole);
                            }
                        }
                    }

                    await uow.CompleteAsync();

                    // Get updated role with permissions
                    var updatedRole = await _roleRepository.GetAsync(roleId);
                    return ObjectMapper.Map<IdentityRole, IdentityRoleDto>(updatedRole);
                }
                catch (Exception ex)
                {
                    await uow.RollbackAsync();
                    throw;
                }
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

        [HttpGet("permissions/all")]
        public async Task<List<PermissionDetailDto>> GetAllPermissions()
        {
            var allPermissions = await _permissionRepository.GetListAsync();

            var result = allPermissions
                .Where(p => p.ParentId == null)
                .Select(parent =>
                {
                    var parentDto = ObjectMapper.Map<W2Permission, PermissionDetailDto>(parent);
                    parentDto.Children = allPermissions
                        .Where(c => c.ParentId == parent.Id)
                        .Select(child => ObjectMapper.Map<W2Permission, PermissionDto>(child))
                        .ToList();
                    return parentDto;
                })
                .ToList();

            return result;
        }
    }
}
