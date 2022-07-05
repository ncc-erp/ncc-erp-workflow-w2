using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using W2.Web.Pages.WorkflowDefinitions.Models;
using W2.WorkflowDefinitions;

namespace W2.Web.Pages.WorkflowDefinitions
{
    public class CreateModalModel : W2PageModel
    {
        private readonly IWorkflowDefinitionAppService _workflowDefinitionAppService;

        public CreateModalModel(IWorkflowDefinitionAppService workflowDefinitionAppService)
        {
            _workflowDefinitionAppService = workflowDefinitionAppService;
        }

        [BindProperty]
        public CreateWorkflowDefinitionViewModel WorkflowDefinition { get; set; }

        public Task OnGetAsync()
        {
            WorkflowDefinition = new CreateWorkflowDefinitionViewModel();

            return Task.CompletedTask;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var workflowDefinitionId = await _workflowDefinitionAppService.CreateWorkflowDefinitionAsync(
                    ObjectMapper.Map<CreateWorkflowDefinitionViewModel, CreateWorkflowDefinitionDto>(WorkflowDefinition)
                );

                return Content(workflowDefinitionId);            
            }

            return BadRequest(ModelState);
        }
    }
}
