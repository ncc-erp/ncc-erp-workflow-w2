using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Security.Claims;
using Volo.Abp.Users;
using W2.ExternalResources;
using W2.Permissions;

namespace W2.Identity
{
    public class CustomClaimsContributor : IAbpClaimsPrincipalContributor, ITransientDependency
    {
        private readonly IProjectClientApi _projectClientApi;
        private readonly ICurrentUser _currentUser;

        public CustomClaimsContributor(IProjectClientApi projectClientApi, ICurrentUser currentUser)
        {
            _projectClientApi = projectClientApi;
            _currentUser = currentUser;
        }

        public async Task ContributeAsync(AbpClaimsPrincipalContributorContext context)
        {
            if (string.IsNullOrEmpty(_currentUser.Email))
            {
                return;
            }

            var projectsInfo = (await _projectClientApi.GetUserProjectsAsync(_currentUser.Email))?.Result;
            if (projectsInfo == null || !projectsInfo.Any())
            {
                return;
            }

            var claimsIdentity = context.ClaimsPrincipal.Identities.FirstOrDefault();
            if (claimsIdentity != null)
            {
                claimsIdentity.AddOrReplace(new Claim(CustomClaim.ProjectName, projectsInfo.FirstOrDefault().Name));
            }
        }
    }
}
