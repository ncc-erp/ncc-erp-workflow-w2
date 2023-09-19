using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IConfiguration _configuration;
        private readonly IdentityRoleManager _identityRoleManager;

        public CustomUserManager(IdentityUserStore store,
            IIdentityRoleRepository roleRepository,
            IIdentityUserRepository userRepository,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<Volo.Abp.Identity.IdentityUser> passwordHasher,
            IEnumerable<IUserValidator<Volo.Abp.Identity.IdentityUser>> userValidators,
            IEnumerable<IPasswordValidator<Volo.Abp.Identity.IdentityUser>> passwordValidators,
            ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<IdentityUserManager> logger,
            ICancellationTokenProvider cancellationTokenProvider,
            IOrganizationUnitRepository organizationUnitRepository,
            ISettingProvider settingProvider,
            IConfiguration configuration, 
            IdentityRoleManager identityRoleManager)
            : base(store, roleRepository, userRepository, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger, cancellationTokenProvider, organizationUnitRepository, settingProvider)
        {
            _configuration = configuration;
            _identityRoleManager = identityRoleManager;
        }

        public override async Task<IdentityResult> CreateAsync(Volo.Abp.Identity.IdentityUser user)
        {
            var identityResult = await base.CreateAsync(user);

            //var defaultDesignerEmails = _configuration.GetSection("DefaultDesignerEmails")
            //    .Get<string[]>()
            //    .Where(x => !x.IsNullOrWhiteSpace())
            //    .ToList();
            //if (defaultDesignerEmails.Any(x => x.Equals(user.Email))
            //    && await _identityRoleManager.RoleExistsAsync(RoleNames.Designer))
            //{
            //    await AddToRoleAsync(user, RoleNames.Designer);
            //}

            return identityResult;
        }
    }
}
