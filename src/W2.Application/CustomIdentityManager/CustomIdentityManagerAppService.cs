using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Identity;
using AutoMapper;
using Volo.Abp;
using W2.Identity;
using W2.Permissions;
using Volo.Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using W2.Roles;
using Volo.Abp.Uow;

namespace W2.CustomIdentityManager
{
    [Authorize]
    public class CustomIdentityManagerAppService: W2AppService, ICustomIdentityManagerAppService
    {
        private readonly IIdentityUserRepository _userRepository;
        private readonly IdentityUserManager _userManager;
        private readonly IRepository<W2PermissionUser, Guid> _permissionUserRepository;
        private readonly IRepository<W2Permission, Guid> _permissionRepository;

        public CustomIdentityManagerAppService(
                IIdentityUserRepository userRepository,
                IdentityUserManager userManager,
                IRepository<W2PermissionUser, Guid> permissionUserRepository,
                IRepository<W2Permission, Guid> permissionRepository
            )
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _permissionUserRepository = permissionUserRepository;
            _permissionRepository = permissionRepository;
        }

        public async Task<PagedResultDto<CustomUserManageDto>> GetListAsync(ListUsersInput input)
        {
            List<IdentityUser> users;

            if (!string.IsNullOrWhiteSpace(input.Roles))
            {
                IList<IdentityUser> user = await _userManager.GetUsersInRoleAsync(input.Roles);
                users = user.ToList();
            }
            else
            {
                users = await _userRepository.GetListAsync();
            }

            if (!string.IsNullOrWhiteSpace(input.Filter))
            {
                string emailRequest = input.Filter.ToLower().Trim();
                users = users.Where(x => x.UserName.Contains(emailRequest)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(input.Sorting))
            {
                users = ApplySorting(users, input.Sorting);
            }

            var config = new MapperConfiguration(cfg => cfg.CreateMap<IdentityUser, CustomUserManageDto>());
            var mapper = config.CreateMapper();
            var userDtos = new List<CustomUserManageDto>();
            foreach (var user in users.Skip(input.SkipCount).Take(input.MaxResultCount))
            {
                var userDto = mapper.Map<CustomUserManageDto>(user);
                var roles = await _userManager.GetRolesAsync(user);
                userDto.Roles = roles.ToList();
                userDtos.Add(userDto);
            }

            return new PagedResultDto<CustomUserManageDto>(users.Count(), userDtos);
        }

        public async Task<List<PermissionDetailDto>> GetUserCustomPermissionsAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new UserFriendlyException($"User with ID {userId} not found.");

            var permissionUsers = await _permissionUserRepository.GetListAsync(
                pu => pu.UserId == userId,
                includeDetails: true
            );

            var permissions = new List<W2Permission>();
            foreach (var permissionUser in permissionUsers)
            {
                var permission = permissionUser.Permission;
                permissions.Add(permission);
            }
            var permissionHierarchy = BuildPermissionHierarchy(permissions);

            return permissionHierarchy;
        }

        public async Task<CustomUserManageDto> UpdateUserAsync(Guid userId, UpdateUserInput input)
        {
            // Get user
            var user = await _userRepository.GetAsync(userId)
                ?? throw new UserFriendlyException($"User with ID {userId} not found.");

            // Validate custom permissions
            var requestedPermissions = await _permissionRepository.GetListAsync(p => input.CustomPermissionIds.Contains(p.Id));
            ValidatePermissions(requestedPermissions, input.CustomPermissionIds);

            using var unitOfWork = UnitOfWorkManager.Begin();

            try
            {
                // Update user
                await _userManager.SetUserNameAsync(user, input.UserName);
                user.Name = input.Name;
                user.Surname = input.Surname;
                await _userManager.SetEmailAsync(user, input.Email);
                await _userManager.SetPhoneNumberAsync(user, input.PhoneNumber);
                user.SetIsActive(input.IsActive);
                await _userManager.SetLockoutEnabledAsync(user, input.LockoutEnabled);
                if (!string.IsNullOrEmpty(input.Password))
                {
                    await _userManager.RemovePasswordAsync(user);
                    await _userManager.AddPasswordAsync(user, input.Password);
                }
                await _userManager.UpdateAsync(user);

                // Update roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                var rolesToRemove = currentRoles.Except(input.RoleNames);
                var rolesToAdd = input.RoleNames.Except(currentRoles);
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                await _userManager.AddToRolesAsync(user, rolesToAdd);

                // Update custom permissions
                var currentPermissions = await _permissionUserRepository.GetListAsync(pu => pu.UserId == userId);
                var permissionsToRemove = currentPermissions.Where(pu => !input.CustomPermissionIds.Contains(pu.PermissionId)).ToList();
                var permissionsToAdd = input.CustomPermissionIds.Where(id => !currentPermissions.Any(pu => pu.PermissionId == id)).ToList();
                foreach (var permissionUser in permissionsToRemove)
                {
                    await _permissionUserRepository.DeleteAsync(permissionUser);
                }
                foreach (var permissionId in permissionsToAdd)
                {
                    var newPermissionUser = new W2PermissionUser(permissionId, userId, CurrentTenant.Id);
                    await _permissionUserRepository.InsertAsync(newPermissionUser);
                }

                // Response
                var userDto = ObjectMapper.Map<IdentityUser, CustomUserManageDto>(user);
                var userRoles = await _userManager.GetRolesAsync(user);
                var userCustomPermissions = await GetUserCustomPermissionsAsync(userId);
                userDto.Roles = userRoles.ToList();
                userDto.CustomPermissions = userCustomPermissions;

                await unitOfWork.CompleteAsync();

                return userDto;
            }
            catch(Exception)
            {
                await unitOfWork.RollbackAsync();
                throw;
            }
        }

        private List<IdentityUser> ApplySorting(List<IdentityUser> users, string sorting)
        {
            if (string.IsNullOrEmpty(sorting))
            {
                return users.OrderBy(u => u.CreationTime).ToList();
            }

            var sortingParts = sorting.Trim().Split(' ');

            if (sortingParts.Length != 2)
            {
                return users.OrderBy(u => u.CreationTime).ToList();
            }

            var property = sortingParts[0].ToLower();
            var direction = sortingParts[1].ToLower();

            if (!string.IsNullOrEmpty(direction) && direction == "asc")
            {
                return users.OrderBy(CreateSortingExpression(property)).ToList();
            }

            return users.OrderByDescending(CreateSortingExpression(property)).ToList();
        }

        private Func<IdentityUser, object> CreateSortingExpression(string property)
        {
            switch (property)
            {
                case "username":
                    return u => u.UserName;
                case "email":
                    return u => u.Email;
                case "phonenumber":
                    return u => u.PhoneNumber;
                default:
                    return u => u.CreationTime;
            }
        }

        private void ValidatePermissions(List<W2Permission> permissions, List<Guid> requestedIds)
        {
            if (permissions.Count != requestedIds.Count)
            {
                throw new UserFriendlyException("Some requested permissions do not exist");
            }

            var parentPermissions = permissions.Count(p => p.ParentId == null);
            var childPermissions = permissions.Count(p => p.ParentId != null);

            if (parentPermissions == 0 || childPermissions == 0)
            {
                throw new UserFriendlyException("At least one permission are required");
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
                var orphanedIds = string.Join(", ", orphanedChildren.Select(p => p.Id));
                throw new UserFriendlyException(
                    $"Child permissions must belong to selected parent permissions. Orphaned permission IDs: {orphanedIds}"
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
