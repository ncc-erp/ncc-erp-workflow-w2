namespace W2.Constants;

public static class W2ApiPermissions
{
    // Workflow Definitions
    public const string WorkflowDefinitionsManagement = "WorkflowDefinitions";
    public const string ViewListWorkflowDefinitions = WorkflowDefinitionsManagement + ".View";
    public const string CreateWorkflowDefinition = WorkflowDefinitionsManagement + ".Create";
    public const string ImportWorkflowDefinition = WorkflowDefinitionsManagement + ".Import";
    public const string UpdateWorkflowDefinitionStatus = WorkflowDefinitionsManagement + ".UpdateStatus";
    public const string DeleteWorkflowDefinition = WorkflowDefinitionsManagement + ".Delete";
    public const string DefineInputWorkflowDefinition = WorkflowDefinitionsManagement + ".DefineInput";
    public const string EditWorkflowDefinition = WorkflowDefinitionsManagement + ".Edit";

    // Workflow Instances
    public const string WorkflowInstancesManagement = "WorkflowInstances";
    public const string ViewListWorkflowInstances = WorkflowInstancesManagement + ".View";
    public const string ViewAllWorkflowInstances = WorkflowInstancesManagement + ".ViewAll";
    public const string CreateWorkflowInstance = WorkflowInstancesManagement + ".Create";
    public const string CancelWorkflowInstance = WorkflowInstancesManagement + ".Cancel";

    // WFH Reporst
    public const string WFHReport = "WFHReport";
    public const string ViewListWFHReport = WFHReport + ".View";

    // Tasks
    public const string TasksManagement = "Tasks";
    public const string ViewListTasks = TasksManagement + ".View";
    public const string ViewAllTasks = TasksManagement + ".ViewAll";
    public const string UpdateTaskStatus = TasksManagement + ".UpdateStatus";
    public const string AssignTask = TasksManagement + ".Assign";

    // Users
    public const string UsersManagement = "Users";
    public const string ViewListUsers = UsersManagement + ".View";
    public const string UpdateUser = UsersManagement + ".Update";

    // Settings
    public const string SettingsManagement = "Settings";
    public const string ViewListSettings = SettingsManagement + ".View";
    public const string CreateSetting = SettingsManagement + ".Create";
    public const string UpdateSetting = SettingsManagement + ".Update";
    public const string DeleteSetting = SettingsManagement + ".Delete";

    // Roles
    public const string RolesManagement = "Roles";
    public const string ViewListRoles = RolesManagement + ".View";
    public const string CreateRole = RolesManagement + ".Create";
    public const string UpdateRole = RolesManagement + ".Update";
    public const string DeleteRole = RolesManagement + ".Delete";
    public const string DeleteUserOnRole = RolesManagement + ".DeleteUserOnRole";

    //Permissions
    public const string PermissionsManagement = "Permissions";
    public const string ViewListPermissions = PermissionsManagement + ".View";
    public const string CreatePermissions = PermissionsManagement + ".Create";
    public const string UpdatePermissions = PermissionsManagement + ".Update";
    public const string DeletePermisisons = PermissionsManagement + ".Delete";

    // Webhooks
    public const string Webhooks = "Webhooks";
    public const string ViewListWebhooks = Webhooks + ".View";
    public const string CreateWebhook = Webhooks + ".Create";
    public const string UpdateWebhook = Webhooks + ".Update";
    public const string DeleteWebhook = Webhooks + ".Delete";

}
