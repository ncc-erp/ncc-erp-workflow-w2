using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using W2.Identity;
using W2.Permissions;
using Volo.Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using W2.Roles;
using Volo.Abp;
using Volo.Abp.Identity;

namespace W2.Users
{
    [Authorize]
    [Route("api/app/users")]
    public class UserAppService : W2AppService, IUserAppService
    {
        private readonly IRepository<W2CustomIdentityUser, Guid> _userRepository;
        private readonly IRepository<W2CustomIdentityRole, Guid> _roleRepository;
        private readonly IRepository<W2Permission, Guid> _permissionRepository;

        public UserAppService(
                IRepository<W2CustomIdentityUser, Guid> userRepository,
                IRepository<W2CustomIdentityRole, Guid> roleRepository,
                IRepository<W2Permission, Guid> permissionRepository
            )
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _permissionRepository = permissionRepository;
        }

        [HttpGet]
        public async Task<PagedResultDto<UserDto>> GetListAsync(ListUsersInput input)
        {
            // Create query with eager loading of roles
            var query = await _userRepository.GetQueryableAsync();
            query = query.Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role);

            // Apply role filter
            if (!string.IsNullOrWhiteSpace(input.Role))
            {
                query = query.Where(u => u.UserRoles.Any(ur => ur.Role.Name == input.Role));
            }

            // Apply email filter
            if (!string.IsNullOrWhiteSpace(input.Filter))
            {
                string filterText = input.Filter.ToLower().Trim();
                query = query.Where(u =>
                    u.Email.ToLower().Contains(filterText)
                );
            }

            // Apply sorting
            query = ApplySorting(query, input.Sorting);

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply paging and execute query
            var users = await query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToListAsync();

            // Response
            var userDtos = ObjectMapper.Map<List<W2CustomIdentityUser>, List<UserDto>>(users);
            return new PagedResultDto<UserDto>(totalCount, userDtos);
        }

        [HttpGet("{userId}/roles")]
        public async Task<List<IdentityRoleDto>> GetUserRolesAsync(Guid userId)
        {
            var query = await _userRepository.GetQueryableAsync();
            var user = await query
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new UserFriendlyException($"Could not find the user with id: {userId}");
            }

            return user.UserRoles.Select(
                ur => ObjectMapper.Map<W2CustomIdentityRole, IdentityRoleDto>(ur.Role)
            ).ToList();
        }

        [HttpGet("{userId}/permissions")]
        public async Task<List<PermissionDetailDto>> GetUserPermissionsAsync(Guid userId)
        {
            var query = await _userRepository.GetQueryableAsync();
            var user = await query
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new UserFriendlyException($"Could not find the user with id: {userId}");
            }

            return user.CustomPermissionDtos;
        }

        [HttpPut("{userId}")]
        public async Task<UserDetailDto> UpdateUserAsync(Guid userId, UpdateUserInput input)
        {
            // Get existing user
            var query = await _userRepository.GetQueryableAsync();
            var user = await query
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new UserFriendlyException($"Could not find the user with id: {userId}");
            }

            // Update basic user properties
            user.SetUsetName(input.UserName);
            user.Name = input.Name;
            user.Surname = input.Surname;
            user.SetEmail(input.Email);
            user.SetPhoneNumber(input.PhoneNumber);
            user.SetLockoutEnabled(input.LockoutEnabled);
            user.SetIsActive(input.IsActive);

            // Update custom permissions
            var permissions = await _permissionRepository
               .GetListAsync(p => input.CustomPermissionCodes.Contains(p.Code));
            var permissionHierarchy = W2Permission.BuildPermissionHierarchy(permissions);            
            user.CustomPermissionDtos = permissionHierarchy;

            // Clear old roles
            user.UserRoles.Clear();

            // Add new roles
            var newRoles = await _roleRepository
                .GetListAsync(r => input.RoleNames.Contains(r.Name));
            foreach (var role in newRoles)
            {
                user.UserRoles.Add(new W2CustomIdentityUserRole(user.Id, role.Id, CurrentTenant.Id));
            }

            // Save changes
            await _userRepository.UpdateAsync(user);

            // Return updated user
            return ObjectMapper.Map<W2CustomIdentityUser, UserDetailDto>(user);
        }

        private IQueryable<W2CustomIdentityUser> ApplySorting(IQueryable<W2CustomIdentityUser> query, string sorting)
        {
            if (string.IsNullOrEmpty(sorting))
            {
                return query.OrderBy(u => u.CreationTime);
            }

            var sortingParts = sorting.Trim().Split(' ');
            if (sortingParts.Length != 2)
            {
                return query.OrderBy(u => u.CreationTime);
            }

            var property = sortingParts[0].ToLower();
            var isAscending = sortingParts[1].ToLower() == "asc";

            query = property switch
            {
                "username" => isAscending
                    ? query.OrderBy(u => u.UserName)
                    : query.OrderByDescending(u => u.UserName),

                "email" => isAscending
                    ? query.OrderBy(u => u.Email)
                    : query.OrderByDescending(u => u.Email),

                "phonenumber" => isAscending
                    ? query.OrderBy(u => u.PhoneNumber)
                    : query.OrderByDescending(u => u.PhoneNumber),

                "custompermissions" => isAscending
                    ? query.OrderBy(u => u.CustomPermissions)
                    : query.OrderByDescending(u => u.CustomPermissions),

                _ => query.OrderBy(u => u.CreationTime)
            };

            return query;
        }
    }
}
