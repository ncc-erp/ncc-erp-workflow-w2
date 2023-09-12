using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Models;
using Elsa.Services;
using Elsa.Services.Models;
using Humanizer;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using W2.ExternalResources;
using W2.Permissions;
using W2.Scripting;
using W2.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace W2.Activities
{
    [Activity(
        DisplayName = "Set RequestUser Variable", 
        Description = "Set RequestUser variable on the workflow.", 
        Category = "Primitives", 
        Outcomes = new string[] { "Done" })]
    public class SetRequestUserVariable : Activity
    {
        private readonly ICurrentUser _currentUser;
        private readonly IProjectClientApi _projectClientApi;
        private readonly IExternalResourceAppService _externalResourceAppService;
        private readonly IRepository<MyTask, Guid> _myTaskRepository;

        public SetRequestUserVariable(ICurrentUser currentUser,
            IProjectClientApi projectClientApi,
            IExternalResourceAppService externalResourceAppService,
            IRepository<MyTask, Guid> myTaskRepository)
        {
            _currentUser = currentUser;
            _projectClientApi = projectClientApi;
            _externalResourceAppService = externalResourceAppService;
            _myTaskRepository = myTaskRepository;
        }

        protected async override ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            // set project
            // set branch info
            var branchResult = await _externalResourceAppService.GetUserBranchInfoAsync(_currentUser.Email);
            //To.Add(branchResult.HeadOfOfficeEmail);
            // set PM
            var userProjectsResult = await _projectClientApi.GetUserProjectsAsync(_currentUser?.Email);
            ProjectProjectItem project = null;
            if (userProjectsResult?.Result != null && userProjectsResult?.Result.Count > 0)
            {
                project = userProjectsResult.Result.First();
            }
            var requestUser = new RequestUser
            {
                Email = _currentUser.Email,
                Name = _currentUser.Name,
                Project = _currentUser.FindClaimValue(CustomClaim.ProjectName),
                HeadOfOfficeEmail = branchResult?.HeadOfOfficeEmail,
                BranchCode = branchResult?.Code,
                BranchName = branchResult?.DisplayName,
                ProjectCode = project?.Code,
                PM = project?.PM?.EmailAddress
            };
            string id = context.WorkflowExecutionContext.WorkflowInstance.Id;
            Console.WriteLine("AAAAAAADDDDDDDDDDDDDDDDDDDDFFFFFFFFFFFFFFFFFFFFGGGGGGGGGGGGGGGGG");
            Console.WriteLine(JsonConvert.SerializeObject(project));
            Console.WriteLine("YOUR ID:::: " + id);

            var newTask = new MyTask { Email = project?.PM?.EmailAddress, Status = project?.Code, WorkflowInstanceId = id };
            await _myTaskRepository.InsertAsync(newTask);

            context.SetVariable(nameof(RequestUser), requestUser);
            return Done();
        }
    }
}
