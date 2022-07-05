using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Volo.Abp.Account;
using Volo.Abp.Account.Web.Pages.Account;
using Volo.Abp.DependencyInjection;

namespace W2.Web.Pages.Account
{
    [Dependency(ReplaceServices = true)]
    [ExposeServices(typeof(RegisterModel))]
    public class CustomRegisterModel : RegisterModel
    {
        public CustomRegisterModel(IAccountAppService accountAppService) : base(accountAppService)
        {
        }
    }
}
