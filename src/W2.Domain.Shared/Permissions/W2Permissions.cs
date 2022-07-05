namespace W2.Permissions;

public static class W2Permissions
{
    //Workflow Management
    public const string WorkflowManagementGroup = "WorkflowManagement";
    public const string WorkflowManagementWorkflowDefinitions = WorkflowManagementGroup + ".WorkflowDefinitions";
    public const string WorkflowManagementWorkflowDefinitionsDesign = WorkflowManagementGroup + ".WorkflowDefinitions.Design";
    public const string WorkflowManagementWorkflowInstances = WorkflowManagementGroup + ".WorkflowInstances";
    public const string WorkflowManagementWorkflowInstancesCreate = WorkflowManagementGroup + ".WorkflowInstances.Create";
    public const string WorkflowManagementWorkflowInstancesViewAll = WorkflowManagementGroup + ".WorkflowInstances.ViewAll";

    //Settings
    public const string WorkflowManagementSettings = "Settings";
    public const string WorkflowManagementSettingsSocialLoginSettings = WorkflowManagementSettings + ".SocialLoginSettings";
}
