using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Services;
using Elsa.Services.Models;
using Volo.Abp.Users;
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

        public SetRequestUserVariable(ICurrentUser currentUser)
        {
            _currentUser = currentUser;
        }

        protected override IActivityExecutionResult OnExecute(ActivityExecutionContext context)
        {
            var requestUser = new RequestUser
            {
                Email = _currentUser.Email,
                Name = _currentUser.Name,
                Project = _currentUser.FindClaimValue(CustomClaim.ProjectName)
            };
            context.SetVariable(nameof(RequestUser), requestUser);
            return Done();
        }
    }
}
