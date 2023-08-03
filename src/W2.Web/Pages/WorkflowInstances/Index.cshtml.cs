using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using W2.WorkflowDefinitions;

namespace W2.Web.Pages.WorkflowInstances
{
    public class IndexModel : PageModel
    {
        private readonly IWorkflowDefinitionAppService _workflowDefinitionAppService;
        public IndexModel(IWorkflowDefinitionAppService workflowDefinitionAppService)
        {
            _workflowDefinitionAppService = workflowDefinitionAppService;
        }

        public IEnumerable<SelectListItem> WorkflowSelectListItems { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            WorkflowSelectListItems = (await _workflowDefinitionAppService.ListAllAsync()).Items
                .AsEnumerable().Select(x => new SelectListItem
                {
                    Value = x.DefinitionId,
                    Text = x.DisplayName
                });

            return Page();
        }
    }
}
