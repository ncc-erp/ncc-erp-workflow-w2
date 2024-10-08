using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Identity;
using AutoMapper;

namespace W2.CustomIdentityManager
{
    [Authorize]
    public class CustomIdentityManagerAppService: W2AppService, ICustomIdentityManagerAppService
    {
        private readonly IIdentityUserRepository _userRepository;
        private readonly IdentityUserManager _userManager;

        public CustomIdentityManagerAppService(
                IIdentityUserRepository userRepository,
                IdentityUserManager userManager
            )
        {
            _userRepository = userRepository;
            _userManager = userManager;
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
    }
}
