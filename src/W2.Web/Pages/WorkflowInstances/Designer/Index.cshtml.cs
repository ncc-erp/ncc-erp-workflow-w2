using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using W2.Permissions;
using W2.WorkflowInstances;

namespace W2.Web.Pages.WorkflowInstances.Designer
{
    public class IndexModel : W2PageModel
    {
        private readonly IWorkflowInstanceAppService _workflowInstanceAppService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IWorkflowInstanceAppService workflowInstanceAppService, ILogger<IndexModel> logger)
        {
            _workflowInstanceAppService = workflowInstanceAppService;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        [FromQuery(Name = "id")]
        public string WorkflowInstanceId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var instance = await _workflowInstanceAppService.GetByIdAsync(WorkflowInstanceId);
            _logger.LogDebug($"Instance Id: {instance.Id}");
            _logger.LogDebug($"Instance Creator Id: {instance.CreatorId}");
            _logger.LogDebug($"Current User Id: {CurrentUser.Id}");
            _logger.LogDebug($"Is Granted: {await AuthorizationService.IsGrantedAsync(W2Permissions.WorkflowManagementWorkflowInstancesViewAll)}");
            if (instance.CreatorId != CurrentUser.Id && !await AuthorizationService.IsGrantedAsync(W2Permissions.WorkflowManagementWorkflowInstancesViewAll))
            {
                _logger.LogError("Error when fetch workflow instance");
                return Unauthorized();
            }

            return Page();
        }
    }
}
