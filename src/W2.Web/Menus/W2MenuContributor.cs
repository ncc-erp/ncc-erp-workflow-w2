using System.Threading.Tasks;
using W2.Localization;
using W2.MultiTenancy;
using Volo.Abp.Identity.Web.Navigation;
using Volo.Abp.SettingManagement.Web.Navigation;
using Volo.Abp.TenantManagement.Web.Navigation;
using Volo.Abp.UI.Navigation;
using W2.Permissions;
using Microsoft.AspNetCore.Authorization;

namespace W2.Web.Menus;

public class W2MenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
    }

    private async Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var administration = context.Menu.GetAdministration();
        var l = context.GetLocalizer<W2Resource>();

        var workflowInstanceMenuText = await context.IsGrantedAsync(W2Permissions.WorkflowManagementWorkflowInstancesViewAll)
            ? l["WorkflowInstance:AllInstances"]
            : l["WorkflowInstance:MyInstances"];

        context.Menu.AddItem(
            new ApplicationMenuItem(W2Menus.WorkflowManagement, l["Menu:WorkflowManagement"])
                .AddItem(new ApplicationMenuItem(
                    name: W2Menus.WorkflowDefinitions,
                    displayName: l["Menu:WorkflowManagement:WorkflowDefinitions"],
                    requiredPermissionName: W2Permissions.WorkflowManagementWorkflowDefinitions,
                    url: "/"
                ))
                .AddItem(new ApplicationMenuItem(
                    name: W2Menus.WorkflowInstances,
                    displayName: workflowInstanceMenuText,
                    requiredPermissionName: W2Permissions.WorkflowManagementWorkflowInstances,
                    url: "/WorkflowInstances/"
                ))
        );

        if (MultiTenancyConsts.IsEnabled)
        {
            administration.SetSubItemOrder(TenantManagementMenuNames.GroupName, 1);
        }
        else
        {
            administration.TryRemoveMenuItem(TenantManagementMenuNames.GroupName);
        }

        administration.SetSubItemOrder(IdentityMenuNames.GroupName, 2);
        administration.SetSubItemOrder(SettingManagementMenuNames.GroupName, 3);
    }
}
