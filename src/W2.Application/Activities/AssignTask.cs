using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Services;
using Elsa.Services.Models;
using Humanizer;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Users;
using W2.ExternalResources;
using W2.Permissions;
using W2.Scripting;

namespace W2.Activities
{
    [Activity(
        DisplayName = "Assign task for user", 
        Description = "Assign task for user on the workflow.", 
        Category = "Primitives", 
        Outcomes = new string[] { "Done" })]
    public class AssignTask : Activity
    {
        private readonly ICurrentUser _currentUser;
        private readonly IProjectClientApi _projectClientApi;
        private readonly IExternalResourceAppService _externalResourceAppService;

        [ActivityInput(Hint = "User's assigned.", SupportedSyntaxes = new string[] { "JavaScript", "Liquid" })]
        public object? User { get; set; }


        public AssignTask(ICurrentUser currentUser,
            IProjectClientApi projectClientApi,
            IExternalResourceAppService externalResourceAppService)
        {
            _currentUser = currentUser;
            _projectClientApi = projectClientApi;
            _externalResourceAppService = externalResourceAppService;
        }

        protected async override ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            // get data from input and get email, assign task to db
            return Done();
        }
    }
}
