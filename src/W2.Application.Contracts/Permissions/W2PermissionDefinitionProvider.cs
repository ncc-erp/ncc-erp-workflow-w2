using W2.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace W2.Permissions;

public class W2PermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var workflowManagementGroup = context.AddGroup(W2Permissions.WorkflowManagementGroup, L("Permissions:WorkflowManagementGroup"));
        var workflowDefinitionsPermission = workflowManagementGroup
            .AddPermission(
                W2Permissions.WorkflowManagementWorkflowDefinitions, 
                L("Permissions:WorkflowDefinitions")
            );
        workflowDefinitionsPermission.AddChild(
            W2Permissions.WorkflowManagementWorkflowDefinitionsDesign,
            L("Permissions:WorkflowDefinitions:Design")
        );
        var workflowInstancesPermission = workflowManagementGroup
            .AddPermission(
                W2Permissions.WorkflowManagementWorkflowInstances,
                L("Permissions:WorkflowInstances")
            );
        workflowInstancesPermission.AddChild(
            W2Permissions.WorkflowManagementWorkflowInstancesCreate,
            L("Permissions:WorkflowInstances:Create")
        );
        var settingPermission = workflowManagementGroup
            .AddPermission(
                W2Permissions.WorkflowManagementSettings,
                L("Permissions:Settings")
            );
        settingPermission.AddChild(
            W2Permissions.WorkflowManagementSettingsSocialLoginSettings,
            L("Permissions:Settings:SocialLoginSettings")
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<W2Resource>(name);
    }
}
