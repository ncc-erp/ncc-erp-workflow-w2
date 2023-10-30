using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using W2.Tasks;
using static Volo.Abp.Identity.Settings.IdentitySettingNames;

namespace W2.CustomIdentityManager
{
    [Authorize]
    public class CustomIdentityManagerAppService: W2AppService, ICustomIdentityManagerAppService
    {
        private readonly IIdentityRoleRepository _roleRepository;
        private readonly IIdentityUserRepository _userRepository;
        //private readonly IRepository<IdentityUserRole> _userRoleRepository;
        private readonly IdentityUserManager _userManager;

        private readonly IOptions<IdentityOptions> _identityOptions;

        public CustomIdentityManagerAppService(
                IIdentityRoleRepository roleRepository,
                IIdentityUserRepository userRepository,
                IdentityUserManager userManager,
                IOptions<IdentityOptions> identityOptions
            )
        {
            _roleRepository = roleRepository;
            _userRepository = userRepository;
            _userManager = userManager;
            _identityOptions = identityOptions;
        }

        public async Task<PagedResultDto<CustomUserManageDto>> GetListAsync(ListUsersInput input)
        {
            List<Volo.Abp.Identity.IdentityUser> users;

            if (!string.IsNullOrWhiteSpace(input.Roles))
            {
                IList<Volo.Abp.Identity.IdentityUser> temp = await _userManager.GetUsersInRoleAsync(input.Roles);
                users = temp.ToList();
            } else
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

            var userDtos = users.Select(MapUserToDto)
                                .Skip(input.SkipCount)
                                .Take(input.MaxResultCount)
                                .ToList();

            return new PagedResultDto<CustomUserManageDto>(users.Count(), userDtos);
        }

        private CustomUserManageDto MapUserToDto(Volo.Abp.Identity.IdentityUser user)
        {
            return new CustomUserManageDto
            {
                TenantId = user.TenantId,
                UserName = user.UserName,
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                IsActive = user.IsActive,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd,
                ConcurrencyStamp = user.ConcurrencyStamp,
                IsDeleted = user.IsDeleted,
                DeleterId = user.DeleterId,
                DeletionTime = user.DeletionTime,
                CreationTime = user.CreationTime,
                CreatorId = user.CreatorId,
                Id = user.Id,
                ExtraProperties = user.ExtraProperties
            };
        }

        private List<Volo.Abp.Identity.IdentityUser> ApplySorting(List<Volo.Abp.Identity.IdentityUser> users, string sorting)
        {
            if (string.IsNullOrEmpty(sorting))
            {
                return users.OrderBy(u => u.CreationTime).ToList(); // Default sorting
            }

            var sortingParts = sorting.Trim().Split(' ');

            if (sortingParts.Length != 2)
            {
                // Handle invalid sorting format
                return users.OrderBy(u => u.CreationTime).ToList(); // Default sorting
            }

            var property = sortingParts[0].ToLower();
            var direction = sortingParts[1].ToLower();

            if (!string.IsNullOrEmpty(direction) && direction == "asc")
            {
                return users.OrderBy(CreateSortingExpression(property)).ToList();
            }

            return users.OrderByDescending(CreateSortingExpression(property)).ToList();
        }

        private Func<Volo.Abp.Identity.IdentityUser, object> CreateSortingExpression(string property)
        {
            switch (property)
            {
                case "createdat":
                    return u => u.CreationTime;
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
    }
}
