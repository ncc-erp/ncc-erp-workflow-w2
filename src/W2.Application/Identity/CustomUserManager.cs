using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.Settings;
using Volo.Abp.Threading;

namespace W2.Identity
{
    [ExposeServices(typeof(IdentityUserManager))]
    public class CustomUserManager : IdentityUserManager
    {
        public CustomUserManager(IdentityUserStore store, IIdentityRoleRepository roleRepository, IIdentityUserRepository userRepository, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<Volo.Abp.Identity.IdentityUser> passwordHasher, IEnumerable<IUserValidator<Volo.Abp.Identity.IdentityUser>> userValidators, IEnumerable<IPasswordValidator<Volo.Abp.Identity.IdentityUser>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<IdentityUserManager> logger, ICancellationTokenProvider cancellationTokenProvider, IOrganizationUnitRepository organizationUnitRepository, ISettingProvider settingProvider) : base(store, roleRepository, userRepository, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger, cancellationTokenProvider, organizationUnitRepository, settingProvider)
        {
        }
    }
}
