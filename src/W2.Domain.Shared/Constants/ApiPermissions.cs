namespace W2.Constants;

public static class W2ApiPermissions
{
    // Workflow Definitions
    public const string WorkflowDefinitionsManagement = "WorkflowDefinitions";
    public const string ViewListWorkflowDefinitions = WorkflowDefinitionsManagement + ".View";
    public const string CreateWorkflowDefinition = WorkflowDefinitionsManagement + ".Create";
    // public const string ImportWorkflowDefinition = WorkflowDefinitionsManagement + ".Import";
    public const string UpdateWorkflowDefinitionStatus = WorkflowDefinitionsManagement + ".UpdateStatus";
    public const string DeleteWorkflowDefinition = WorkflowDefinitionsManagement + ".Delete";

    // Workflow Instances
    public const string WorkflowInstancesManagement = "WorkflowInstances";
    public const string ViewListWorkflowInstances = WorkflowInstancesManagement + ".View";
    public const string CreateWorkflowInstance = WorkflowInstancesManagement + ".Create";
    public const string CancelWorkflowInstance = WorkflowInstancesManagement + ".Cancel";

    // Tasks
    public const string TasksManagement = "Tasks";
    public const string ViewListTasks = TasksManagement + ".View";
    public const string UpdateTaskStatus = TasksManagement + ".UpdateStatus";
    public const string AssignTask = TasksManagement + ".Assign";

    // Users
    public const string UsersManagement = "Users";
    public const string ViewListUsers = UsersManagement + ".View";
    public const string UpdateUser = UsersManagement + ".Update";

    // Settings
    public const string SettingsManagement = "Settings";
    public const string ViewSettings = SettingsManagement + ".View";
    public const string CreateSettings = SettingsManagement + ".Create";
    public const string UpdateSettings = SettingsManagement + ".Update";

    // Roles
    public const string RolesManagement = "Roles";
    public const string ViewListRoles = RolesManagement + ".View";
    public const string CreateRole = RolesManagement + ".Create";
    public const string UpdateRole = RolesManagement + ".Update";
}
