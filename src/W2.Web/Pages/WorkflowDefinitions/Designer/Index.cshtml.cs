using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using W2.Permissions;

namespace W2.Web.Pages.WorkflowDefinitions.Designer
{
    [Authorize(W2Permissions.WorkflowManagementWorkflowDefinitionsDesign)]
    public class IndexModel : W2PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string WorkflowDefinitionId { get; set; }

        public void OnGet()
        {
        }
    }
}
