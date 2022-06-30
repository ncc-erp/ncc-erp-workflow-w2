using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using W2.Permissions;

namespace W2.Web.Pages.WorkflowDefinitions
{
    [Authorize(W2Permissions.WorkflowManagementWorkflowDefinitions)]
    public class IndexModel : W2PageModel
    {
        public void OnGet()
        {
        }
    }
}
