using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;
using Volo.Abp.Account.Web;
using Volo.Abp.Account.Web.Pages.Account;
using Volo.Abp.Security.Claims;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace W2.Web.Pages.Account
{
    public class CustomLoginModel : LoginModel
    {
        public CustomLoginModel(IAuthenticationSchemeProvider schemeProvider, IOptions<AbpAccountOptions> accountOptions, IOptions<IdentityOptions> identityOptions) : base(schemeProvider, accountOptions, identityOptions)
        {
        }

        public override Task<IActionResult> OnGetExternalLoginCallbackAsync(string returnUrl = "", string returnUrlHash = "", string remoteError = null)
        {
            return base.OnGetExternalLoginCallbackAsync(returnUrl, returnUrlHash, remoteError);
        }
    }
}
