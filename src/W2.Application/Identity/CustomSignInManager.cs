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
    public class CustomSignInManager : SignInManager<W2CustomIdentityUser>
    {
        public CustomSignInManager(UserManager<W2CustomIdentityUser> userManager, 
            IHttpContextAccessor contextAccessor, 
            IUserClaimsPrincipalFactory<W2CustomIdentityUser> claimsFactory, 
            IOptions<IdentityOptions> optionsAccessor, 
            ILogger<SignInManager<W2CustomIdentityUser>> logger, 
            IAuthenticationSchemeProvider schemes, 
            IUserConfirmation<W2CustomIdentityUser> confirmation) 
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
        }
    }
}
