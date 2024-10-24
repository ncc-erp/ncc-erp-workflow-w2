using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using W2.Permissions;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using W2.Identity;

namespace W2.Roles
{
    [Authorize]
    [Route("api/app/roles")]
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
        public async Task<ListResultDto<IdentityRoleDto>> GetRolesAsync()
        {
            var roles = await _roleRepository.GetListAsync();
            return new ListResultDto<IdentityRoleDto>(
                ObjectMapper.Map<List<W2CustomIdentityRole>, List<IdentityRoleDto>>(roles)
            );
        }

        [HttpGet("{id}")]
        public async Task<RoleDetailDto> GetRoleDetailsAsync(Guid id)
        {
            var role = await _roleRepository.GetAsync(id)
                ?? throw new UserFriendlyException($"Role with id {id} not found");

            var roleDetailsDto = ObjectMapper.Map<W2CustomIdentityRole, RoleDetailDto>(role);
            roleDetailsDto.Permissions = role.PermissionDtos;

            return roleDetailsDto;
        }

        [HttpPost]
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
            var roleDetailsDto = ObjectMapper.Map<W2CustomIdentityRole, RoleDetailDto>(role);
            roleDetailsDto.Permissions = permissionHierarchy;

            return roleDetailsDto;
        }

        [HttpPut("{roleId}")]
        public async Task<RoleDetailDto> UpdateRoleAsync(Guid roleId, UpdateRoleInput input)
        {
            if (input.PermissionCodes == null || !input.PermissionCodes.Any())
            {
                throw new UserFriendlyException("At least one permission are required");
            }

            // Get role
            var role = await _roleRepository.GetAsync(roleId)
                    ?? throw new UserFriendlyException($"Role with id {roleId} not found");

            // Set up permisisons
            var permissions = await _permissionRepository
                .GetListAsync(p => input.PermissionCodes.Contains(p.Code));
            var permissionHierarchy = W2Permission.BuildPermissionHierarchy(permissions);

            // Update role
            if (!string.IsNullOrEmpty(input.Name) && role.Name != input.Name)
            {
                role.ChangeName(input.Name);
                role.PermissionDtos = permissionHierarchy;
                await _roleRepository.UpdateAsync(role);
            }

            // Response
            var roleDetailDto = ObjectMapper.Map<W2CustomIdentityRole, RoleDetailDto>(role);
            roleDetailDto.Permissions = permissionHierarchy;

            return roleDetailDto;
        }

        [HttpGet("permissions")]
        public async Task<List<PermissionDetailDto>> GetPermissionsAsync()
        {
            var allPermissions = await _permissionRepository.GetListAsync();
            return W2Permission.BuildPermissionHierarchy(allPermissions);
        }

        //[HttpPost("permissions")]
        //public async Task<IActionResult> CreatePermissionAsync(List<CreatePermissionInput> inputs)
        //{
        //    var permissions = inputs.Select(input => new W2Permission(
        //        input.Name,
        //        input.Code,
        //        input.ParentId == Guid.Empty ? null : input.ParentId,
        //        CurrentTenant.Id
        //    )).ToList();

        //    await _permissionRepository.InsertManyAsync(permissions);

        //    return new OkObjectResult(permissions);
        //}
    }
}
