using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace W2.Identity
{
    public class CustomSignInManager : SignInManager<Volo.Abp.Identity.IdentityUser>
    {
        public CustomSignInManager(UserManager<Volo.Abp.Identity.IdentityUser> userManager, 
            IHttpContextAccessor contextAccessor, 
            IUserClaimsPrincipalFactory<Volo.Abp.Identity.IdentityUser> claimsFactory, 
            IOptions<IdentityOptions> optionsAccessor, 
            ILogger<SignInManager<Volo.Abp.Identity.IdentityUser>> logger, 
            IAuthenticationSchemeProvider schemes, 
            IUserConfirmation<Volo.Abp.Identity.IdentityUser> confirmation) 
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
        }
    }
}
