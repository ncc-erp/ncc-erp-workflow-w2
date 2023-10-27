using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<PagedResultDto<CustomUserManageDto>> ListAsync()
        {
            List<Volo.Abp.Identity.IdentityUser> users;

            // if no have roles filter
            //if (true)
            //{
            //    //user = await _userRepository.GetRolesAsync(Guid.Parse("3a06d34a-fd58-04ec-4a40-c81f16ae69d5"));
            //    users = await _userRepository.GetListAsync();
            //
            //} 

            // users = await _userManager.GetUsersInRoleAsync("DefaultUser");
            IList<Volo.Abp.Identity.IdentityUser> temp = await _userManager.GetUsersInRoleAsync("DefaultUser");
            // convert IList to List
            users = temp.ToList();
            var userDtos = users.Select(MapUserToDto).ToList();

            //var role = await _roleRepository.GetListAsync();
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
    }
}
