using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.TenantManagement;
using W2.Permissions;

namespace W2.Identity
{
    public class IdentityDataSeedContributor : IDataSeedContributor, ITransientDependency
    {
        private readonly IdentityRoleManager _identityRoleManager;
        private readonly IGuidGenerator _guidGenerator;
        private readonly IPermissionManager _permissionManager;
        private readonly Configurations.TenantConfiguration _tenantConfiguration;
        private readonly ITenantRepository _tenantRepository;
        private readonly TenantManager _tenantManager;

        public IdentityDataSeedContributor(IdentityRoleManager identityRoleManager,
            IGuidGenerator guidGenerator,
            IPermissionManager permissionManager,
            IOptions<Configurations.TenantConfiguration> tenantConfigurationOptions,
            ITenantRepository tenantRepository,
            TenantManager tenantManager)
        {
            _identityRoleManager = identityRoleManager;
            _guidGenerator = guidGenerator;
            _permissionManager = permissionManager;
            _tenantConfiguration = tenantConfigurationOptions.Value;
            _tenantRepository = tenantRepository;
            _tenantManager = tenantManager;
        }

        public async Task SeedAsync(DataSeedContext context)
        {
            if (!context.TenantId.HasValue)
            {
                await SeedDefaultTenantsAsync();
            }
            await SeedDefaultRoleAsync(context);
            await SeedDesignerRoleAsync(context);
        }

        private async Task SeedDefaultTenantsAsync()
        {
            if (await _tenantRepository.GetCountAsync() == 0 && _tenantConfiguration.DefaultTenants != null)
            {
                var defaultTenantNames = _tenantConfiguration.DefaultTenants;
                var tenants = new List<Tenant>();
                foreach (var tenantName in defaultTenantNames)
                {
                    tenants.Add(await _tenantManager.CreateAsync(tenantName));

                }

                await _tenantRepository.InsertManyAsync(tenants);
            }
        }

        private async Task SeedDefaultRoleAsync(DataSeedContext context)
        {
            if (await _identityRoleManager.RoleExistsAsync(RoleNames.DefaultUser))
            {
                return;
            }

            var defaultRole = new IdentityRole(
                _guidGenerator.Create(),
                RoleNames.DefaultUser,
                context.TenantId
            )
            {
                IsDefault = true,
                IsPublic = true
            };
            await _identityRoleManager.CreateAsync(defaultRole);

            await _permissionManager.SetForRoleAsync(RoleNames.DefaultUser, W2Permissions.WorkflowManagementWorkflowDefinitions, true);
            await _permissionManager.SetForRoleAsync(RoleNames.DefaultUser, W2Permissions.WorkflowManagementWorkflowInstances, true);
            await _permissionManager.SetForRoleAsync(RoleNames.DefaultUser, W2Permissions.WorkflowManagementWorkflowInstancesCreate, true);
        }

        private async Task SeedDesignerRoleAsync(DataSeedContext context)
        {
            if (await _identityRoleManager.RoleExistsAsync(RoleNames.Designer))
            {
                return;
            }

            var designerRole = new IdentityRole(
                _guidGenerator.Create(),
                RoleNames.Designer,
                context.TenantId
            )
            {
                IsPublic = true
            };
            await _identityRoleManager.CreateAsync(designerRole);

            await _permissionManager.SetForRoleAsync(RoleNames.Designer, W2Permissions.WorkflowManagementWorkflowDefinitions, true);
            await _permissionManager.SetForRoleAsync(RoleNames.Designer, W2Permissions.WorkflowManagementWorkflowDefinitionsDesign, true);
        }
    }
}
