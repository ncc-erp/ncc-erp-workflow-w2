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
using Volo.Abp;
using Volo.Abp.Identity;
using Microsoft.AspNetCore.Identity;
using W2.Authorization.Attributes;
using W2.Constants;
using W2.Roles;
using W2.ExternalResources;
using Volo.Abp.Guids;
using W2.Utils;
using Microsoft.AspNetCore.Authorization;

namespace W2.Users
{
    [Route("api/app/users")]
    [Authorize]
    //[RequirePermission(W2ApiPermissions.UsersManagement)]
    public class UserAppService : W2AppService, IUserAppService
    {
        private readonly IdentityUserManager _userManager;
        private readonly IRepository<W2CustomIdentityUser, Guid> _userRepository;
        private readonly IRepository<W2CustomIdentityRole, Guid> _roleRepository;
        private readonly IRepository<W2Permission, Guid> _permissionRepository;
        private readonly IHrmClientApi _hrmClient;
        private readonly SimpleGuidGenerator _simpleGuidGenerator;

        public UserAppService(
                IdentityUserManager userManager,
                IRepository<W2CustomIdentityUser, Guid> userRepository,
                IRepository<W2CustomIdentityRole, Guid> roleRepository,
                IRepository<W2Permission, Guid> permissionRepository,
                IHrmClientApi hrmClient
            )
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _permissionRepository = permissionRepository;
            _hrmClient = hrmClient;
            _simpleGuidGenerator = SimpleGuidGenerator.Instance;
        }

        [HttpGet]
        [RequirePermission(W2ApiPermissions.ViewListUsers)]
        public async Task<PagedResultDto<UserDto>> GetListAsync(ListUsersInput input)
        {
            // Create query with eager loading of roles
            var query = await _userRepository.GetQueryableAsync();
            query = query.Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role);

            // Apply role filter
            if (input.Role == "empty")
            {
                query = query.Where(u => !u.UserRoles.Any());
            }
            else if (!string.IsNullOrWhiteSpace(input.Role))
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
        [RequirePermission(W2ApiPermissions.ViewListUsers)]
        public async Task<UserRolesDto> GetUserRolesAsync(Guid userId)
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

            var rolesDto = user.UserRoles.Select(
                ur => ObjectMapper.Map<W2CustomIdentityRole, RoleDto>(ur.Role)
            ).ToList();

            return new UserRolesDto
            {
                Items = rolesDto
            };
        }

        [HttpGet("{userId}/permissions")]
        [RequirePermission(W2ApiPermissions.ViewListUsers)]
        public async Task<UserPermissionsDto> GetUserPermissionsAsync(Guid userId)
        {
            var query = await _userRepository.GetQueryableAsync();
            var user = await query
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new UserFriendlyException($"Could not find the user with id: {userId}");
            }

            return new UserPermissionsDto
            {
                Permissions = user.CustomPermissionDtos
            };
        }

        [HttpPut("{userId}")]
        [RequirePermission(W2ApiPermissions.UpdateUser)]
        public async Task UpdateUserAsync(Guid userId, UpdateUserInput input)
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
            user.SetUserName(input.UserName);
            user.Name = input.Name;
            user.Surname = input.Surname;
            user.SetEmail(input.Email);
            user.SetPhoneNumber(input.PhoneNumber);
            user.SetLockoutEnabled(input.LockoutEnabled);
            user.SetIsActive(input.IsActive);
            user.SetMezonUserId(input.MezonUserId);

            if (!input.Password.IsNullOrEmpty())
            {
                (await _userManager.RemovePasswordAsync(user)).CheckErrors();
                (await _userManager.AddPasswordAsync(user, input.Password)).CheckErrors();
            }

            // Update custom permissions
            var permissions = await _permissionRepository
               .GetListAsync(p => input.CustomPermissionCodes.Contains(p.Code));
            var permissionHierarchy = W2Permission.BuildPermissionHierarchy(permissions);
            user.CustomPermissionDtos = permissionHierarchy;

            // Save changes
            await _userRepository.UpdateAsync(user);

            // Clear old roles
            user.UserRoles.Clear();

            // Add new roles
            var newRoles = await _roleRepository
                .GetListAsync(r => input.RoleNames.Contains(r.Name));
            foreach (var role in newRoles)
            {
                user.UserRoles.Add(new W2CustomIdentityUserRole(user.Id, role.Id, CurrentTenant.Id));
            }
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
                    ? query.OrderBy(u => string.IsNullOrEmpty(u.PhoneNumber) ? 1 : 0)
                        .ThenBy(u => u.PhoneNumber ?? "")
                    : query.OrderBy(u => string.IsNullOrEmpty(u.PhoneNumber) ? 1 : 0)
                        .ThenByDescending(u => u.PhoneNumber ?? ""),

                "custompermissions" => isAscending
                    ? query.OrderBy(u => u.CustomPermissions)
                    : query.OrderByDescending(u => u.CustomPermissions),

                "creationtime" => isAscending
                    ? query.OrderBy(u => u.CreationTime)
                    : query.OrderByDescending(u => u.CreationTime),

                "lastmodificationtime" => isAscending
                    ? query.OrderBy(u => u.LastModificationTime)
                    : query.OrderByDescending(u => u.LastModificationTime),

                "mezonuserid" => isAscending
                    ? query.OrderBy(u => string.IsNullOrEmpty(u.MezonUserId) ? 1 : 0)
                        .ThenBy(u => u.MezonUserId ?? "")
                    : query.OrderBy(u => string.IsNullOrEmpty(u.MezonUserId) ? 1 : 0)
                        .ThenByDescending(u => u.MezonUserId ?? ""),

                "roles" => isAscending
                    ? query.OrderBy(u => u.UserRoles.Any() ? 0 : 1)
                        .ThenBy(u => u.UserRoles.Count())
                    : query.OrderBy(u => u.UserRoles.Any() ? 0 : 1)
                        .ThenByDescending(u => u.UserRoles.Count()),


                _ => query.OrderBy(u => u.CreationTime),
            };

            return query;
        }

        [HttpPost("sync-hrm")]
        [RequirePermission(W2ApiPermissions.UpdateUser)]
        public async Task SyncHrmUsers()
        {
            await this.InternalSyncHrmUsers();
        }
        [NonAction]
        public async Task InternalSyncHrmUsers()
        {
            try
            {
                var response = await _hrmClient.GetAllEmployee();
                var allEmployees = response.Result;
                await BulkUpdateUsersAsync(allEmployees);
            }
            catch (Exception)
            {
                throw new UserFriendlyException("Fail to sync HRM Users");
            }
        }
        [NonAction]
        public async Task<List<W2CustomIdentityUser>> BulkUpdateUsersAsync(List<HrmEmployeeInfo> hrmUsers)
        {
            if (hrmUsers == null || hrmUsers.Count == 0)
            {
                throw new UserFriendlyException("No user data provided.");
            }

            // Get the list of users that need to be updated based on email
            var emails = hrmUsers.Select(u => u.Email).ToList();  // Extract emails from the input
            var usersQuery = await _userRepository.GetQueryableAsync();
            var existingUsers  = new List<W2CustomIdentityUser>();
            var newUsers = new List<W2CustomIdentityUser>();

            //Process each user for update
            // filter active users first// todo fixed for all users
            foreach (var userInput in hrmUsers.Where(u => u.Status == 1))
            {
                var existingUser = await usersQuery
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Where(u => userInput.Email == u.Email)
                .FirstOrDefaultAsync();

                if (existingUser != null)
                {
                    // User exists, update their data
                    existingUser.SetUserName(userInput.Email);
                    existingUser.SetEmail(userInput.Email);
                    existingUser.SetMezonUserId(userInput.MezonUserId);
                    existingUser.SetIsActive(userInput.Status == 1);
                    existingUsers.Add(existingUser);
                }
                else
                {
                    // User does not exist, create a new one
                    var existedUser = await _userManager.FindByEmailAsync(userInput.Email);
                    if (existedUser != null) continue; // Skip if user already exists

                    var newUser = new W2CustomIdentityUser(_simpleGuidGenerator.Create(), userInput.Email, userInput.Email);
                    newUser.Name = Helper.ConvertVietnameseToUnsign(userInput.FullName);

                    newUser.SetMezonUserId(userInput.MezonUserId);
                    newUser.SetIsActive(userInput.Status == 1);
                    await _userManager.CreateAsync(newUser);
                    await _userManager.AddToRoleAsync(newUser, RoleNames.DefaultUser);
                    await _userManager.AddDefaultRolesAsync(newUser);
                    newUsers.Add(newUser);
                }
            }

            // Update existing users in the database
            if (existingUsers.Any())
            {
                await _userRepository.UpdateManyAsync(existingUsers);
            }

            return existingUsers.Concat(newUsers).ToList();
        }

    }
}
