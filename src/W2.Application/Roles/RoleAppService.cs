﻿using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using W2.Permissions;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using W2.Identity;
using System.Reflection;
using W2.Authorization.Attributes;
using W2.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using W2.Users;

namespace W2.Roles
{
    [Route("api/app/roles")]
    //[Authorize]
    public class RoleAppService : W2AppService, IRoleAppService
    {
        private readonly IRepository<W2CustomIdentityRole, Guid> _roleRepository;
        private readonly IRepository<W2Permission, Guid> _permissionRepository;

        public RoleAppService(
            IRepository<W2CustomIdentityRole, Guid> roleRepository,
            IRepository<W2Permission, Guid> permissionRepository
        )
        {
            _roleRepository = roleRepository;
            _permissionRepository = permissionRepository;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ListResultDto<RoleDto>> GetRolesAsync()
        {
            var query = await _roleRepository.GetQueryableAsync();
            var roles = await query
                .OrderByDescending(r => r.LastModificationTime)
                .ToListAsync();

            return new ListResultDto<RoleDto>(
                ObjectMapper.Map<List<W2CustomIdentityRole>, List<RoleDto>>(roles)
            );
        }

        [HttpGet("{id}")]
        [RequirePermission(W2ApiPermissions.ViewListRoles)]
        public async Task<RoleDetailDto> GetRoleDetailAsync(Guid id)
        {
            var query = await _roleRepository.GetQueryableAsync();
            var role = await query.Include(r => r.UserRoles)
                .ThenInclude(ur => ur.User)
                .FirstOrDefaultAsync(r => r.Id == id)
                ?? throw new UserFriendlyException($"Role with id {id} not found");

            var roleDetailDto = ObjectMapper.Map<W2CustomIdentityRole, RoleDetailDto>(role);
            roleDetailDto.Permissions = role.PermissionDtos;

            // Map users from UserRoles
            roleDetailDto.Users = role.UserRoles
                .Select(ur => ObjectMapper.Map<W2CustomIdentityUser, UserDto>(ur.User))
                .ToList();

            return roleDetailDto;
        }

        [HttpDelete("{roleId}/users/{userId}")]
        [RequirePermission(W2ApiPermissions.DeleteUserOnRole)]
        public async Task<IActionResult> RemoveUserFromRoleAsync(Guid roleId, Guid userId)
        {
            var dbContext = await _roleRepository.GetDbContextAsync();

            var role = await dbContext.Set<W2CustomIdentityRole>()
                .Include(r => r.UserRoles)
                .FirstOrDefaultAsync(r => r.Id == roleId)
                ?? throw new UserFriendlyException($"Role with id {roleId} not found");

            var userRole = role.UserRoles.FirstOrDefault(ur => ur.UserId == userId);

            if (userRole == null)
            {
                throw new UserFriendlyException($"User with id {userId} not found in role with id {roleId}");
            }

            role.UserRoles.Remove(userRole);
            dbContext.Update(role);
            await dbContext.SaveChangesAsync();

            return new StatusCodeResult(204);
        }

        [HttpPost]
        [RequirePermission(W2ApiPermissions.CreateRole)]
        public async Task<RoleDetailDto> CreateRoleAsync(CreateRoleInput input)
        {
            if (input.PermissionCodes == null || !input.PermissionCodes.Any())
            {
                throw new UserFriendlyException("At least one permission are required");
            }

            // Set up permissions
            var permissions = await _permissionRepository.GetListAsync(
                    p => input.PermissionCodes.Contains(p.Code)
                );
            var permissionHierarchy = W2Permission.BuildPermissionHierarchy(permissions);

            // Create role
            var role = new W2CustomIdentityRole(
                GuidGenerator.Create(),
                input.Name,
                CurrentTenant.Id
            );
            role.PermissionDtos = permissionHierarchy;
            await _roleRepository.InsertAsync(role);

            // Response
            var roleDetailDto = ObjectMapper.Map<W2CustomIdentityRole, RoleDetailDto>(role);
            roleDetailDto.Permissions = permissionHierarchy;

            return roleDetailDto;
        }

        [HttpPut("{roleId}")]
        [RequirePermission(W2ApiPermissions.UpdateRole)]
        public async Task<RoleDetailDto> UpdateRoleAsync(Guid roleId, UpdateRoleInput input)
        {
            if (input.PermissionCodes == null || !input.PermissionCodes.Any())
            {
                throw new UserFriendlyException("At least one permission are required");
            }

            // Get role
            var role = await _roleRepository.GetAsync(roleId)
                    ?? throw new UserFriendlyException($"Role with id {roleId} not found");

            // Set up permissions
            var permissions = await _permissionRepository
                .GetListAsync(p => input.PermissionCodes.Contains(p.Code));
            var permissionHierarchy = W2Permission.BuildPermissionHierarchy(permissions);

            // Update role
            role.PermissionDtos = permissionHierarchy;
            role.SetLastModificationTime(DateTime.UtcNow);
            role.SetName(input.Name);

            // Save changes
            await _roleRepository.UpdateAsync(role);

            // Response
            var roleDetailDto = ObjectMapper.Map<W2CustomIdentityRole, RoleDetailDto>(role);
            roleDetailDto.Permissions = permissionHierarchy;

            return roleDetailDto;
        }

        [HttpGet("permissions")]
        [AllowAnonymous]
        public async Task<List<PermissionDetailDto>> GetPermissionsAsync()
        {
            var allPermissions = await _permissionRepository.GetListAsync();
            return W2Permission.BuildPermissionHierarchy(allPermissions);
        }

        [HttpPost("all-permissions")]
        [RequirePermission(W2ApiPermissions.RolesManagement)]
        public async Task<List<PermissionDetailDto>> SeedPermissionsAsync()
        {
            await _permissionRepository.DeleteAsync(p => true);

            var permissions = new List<W2Permission>();
            var type = typeof(W2ApiPermissions);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            var parentPermissions = new Dictionary<string, Guid>();
            foreach (var field in fields)
            {
                if (field.IsLiteral && !field.IsInitOnly)
                {
                    var code = field.GetValue(null).ToString();
                    if (!code.Contains(".")) 
                    {
                        var id = Guid.NewGuid();
                        var permission = new W2Permission(
                            FormatNameFromVariableName(field.Name),
                            code,
                            null,
                            CurrentTenant.Id
                        );
                        permission.SetId(id);
                        parentPermissions[code] = id;
                        permissions.Add(permission);
                    }
                }
            }

            foreach (var field in fields)
            {
                if (field.IsLiteral && !field.IsInitOnly)
                {
                    var code = field.GetValue(null).ToString();
                    if (code.Contains("."))
                    {
                        var parts = code.Split('.');
                        var parentCode = parts[0];
                        if (parentPermissions.TryGetValue(parentCode, out var parentId))
                        {
                            var permission = new W2Permission(
                                FormatNameFromVariableName(field.Name),
                                code,
                                parentId,
                                CurrentTenant.Id
                            );
                            permission.SetId(Guid.NewGuid());
                            permissions.Add(permission);
                        }
                    }
                }
            }

            await _permissionRepository.InsertManyAsync(permissions);

            return await GetPermissionsAsync();
        }

        private string FormatNameFromVariableName(string variableName)
        {
            var words = System.Text.RegularExpressions.Regex.Replace(variableName, "([A-Z])", " $1").Trim().Split(' ');
            return string.Join(" ", words);
        }

        [HttpDelete("{roleId}")]
        [RequirePermission(W2ApiPermissions.DeleteRole)]
        public async Task DeleteAsync(Guid roleId)
        {
            var role = await _roleRepository.GetAsync(roleId)
                ?? throw new UserFriendlyException($"Role with id {roleId} not found");
            await _roleRepository.DeleteAsync(role);
        }

    }
}
