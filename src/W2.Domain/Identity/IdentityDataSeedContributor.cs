using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;
using W2.Permissions;

namespace W2.Identity
{
    public class IdentityDataSeedContributor : IDataSeedContributor, ITransientDependency
    {
        private readonly IdentityRoleManager _identityRoleManager;
        private readonly IGuidGenerator _guidGenerator;
        private readonly ICurrentTenant _currentTenant;
        private readonly IPermissionManager _permissionManager;

        public IdentityDataSeedContributor(IdentityRoleManager identityRoleManager, IGuidGenerator guidGenerator, ICurrentTenant currentTenant, IPermissionManager permissionManager)
        {
            _identityRoleManager = identityRoleManager;
            _guidGenerator = guidGenerator;
            _currentTenant = currentTenant;
            _permissionManager = permissionManager;
        }

        public async Task SeedAsync(DataSeedContext context)
        {
            using (_currentTenant.Change(context.TenantId))
            {
                if (!await _identityRoleManager.RoleExistsAsync(RoleNames.DefaultUser))
                {
                    var defaultRole = new Volo.Abp.Identity.IdentityRole(
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
            }
        }
    }
}
