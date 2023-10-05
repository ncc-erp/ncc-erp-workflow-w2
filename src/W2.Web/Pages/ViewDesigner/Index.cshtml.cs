using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using W2.Permissions;
using W2.WorkflowInstances;

namespace W2.Web.Pages.ViewDesigner
{
    public class IndexModel : W2PageModel
    {
        private readonly IWorkflowInstanceAppService _workflowInstanceAppService;

        public IndexModel(IWorkflowInstanceAppService workflowInstanceAppService)
        {
            _workflowInstanceAppService = workflowInstanceAppService;
        }

        [BindProperty(SupportsGet = true)]
        [FromQuery(Name = "id")]
        public string WorkflowInstanceId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var instance = await _workflowInstanceAppService.GetByIdAsync(WorkflowInstanceId);

            return Page();
        }
    }
}
