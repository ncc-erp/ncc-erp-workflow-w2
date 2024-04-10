using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Services;
using Elsa.Services.Models;
using Humanizer;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Users;
using W2.ExternalResources;
using W2.Permissions;
using W2.Scripting;

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

        public SetRequestUserVariable(ICurrentUser currentUser,
            IProjectClientApi projectClientApi,
            IExternalResourceAppService externalResourceAppService)
        {
            _currentUser = currentUser;
            _projectClientApi = projectClientApi;
            _externalResourceAppService = externalResourceAppService;
        }

        protected async override ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            string targetStaffEmail = null;
            string shortTitle = null;
            string requestId = null;
            try
            {
                // Check if context and context.Input are not null
                if (context != null && context.Input != null)
                {
                    var bodyProperty = context.Input.GetType().GetProperty("Body");

                    // Check if the 'Body' property exists and is not null
                    if (bodyProperty != null)
                    {
                        var instanceInput = bodyProperty.GetValue(context.Input);

                        if (instanceInput is IDictionary<string, string> valueDictionary && valueDictionary.ContainsKey("shortHeader"))
                        {
                            var key = valueDictionary["shortHeader"];
                            shortTitle = valueDictionary.ContainsKey(key) ?  valueDictionary[key] : "";
                        }

                        if (instanceInput is IDictionary<string, string> dictionary && dictionary.ContainsKey("Staff"))
                        {
                            targetStaffEmail = dictionary["Staff"];
                        }

                        if(instanceInput is IDictionary<string, string> dictionaryRequest && dictionaryRequest.ContainsKey("requestId"))
                        {
                            requestId = dictionaryRequest["requestId"];
                        }

                        
                    }
                }
            }
            catch (Exception)
            {
                targetStaffEmail = null;
                shortTitle = "";
            }

            // set project
            // set branch info
            var branchResult = await _externalResourceAppService.GetUserBranchInfoAsync(_currentUser.Email);
            //To.Add(branchResult.HeadOfOfficeEmail);
            // set PM
            var userProjectsResult = await _projectClientApi.GetUserProjectsAsync(_currentUser.Email);
            ProjectProjectItem project = null;
            if (userProjectsResult?.Result != null && userProjectsResult?.Result.Count > 0)
            {
                project = userProjectsResult.Result.First();
            }
            var requestUser = new RequestUser
            {
                Id = _currentUser.Id,
                Email = _currentUser.Email,
                Name = _currentUser.Name,
                TargetStaffEmail = targetStaffEmail,
                Project = _currentUser.FindClaimValue(CustomClaim.ProjectName),
                HeadOfOfficeEmail = branchResult?.HeadOfOfficeEmail,
                BranchCode = branchResult?.Code,
                BranchName = branchResult?.DisplayName,
                ProjectCode = project?.Code,
                ShortTitle = shortTitle,
                PM = project?.PM?.EmailAddress,
                requestId = Guid.Parse(requestId)
            };
            context.SetVariable(nameof(RequestUser), requestUser);
            return Done();
        }
    }
}
