using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using W2.Permissions;
using W2.Web.Pages.WorkflowDefinitions.Models;
using W2.WorkflowDefinitions;

namespace W2.Web.Pages.WorkflowDefinitions
{
    [Authorize(W2Permissions.WorkflowManagementWorkflowDefinitionsDesign)]
    public class DefineWorkflowInputModalModel : W2PageModel
    {
        private readonly IWorkflowDefinitionAppService _workflowDefinitionAppService;

        public DefineWorkflowInputModalModel(IWorkflowDefinitionAppService workflowDefinitionAppService)
        {
            _workflowDefinitionAppService = workflowDefinitionAppService;
        }

        [BindProperty]
        public DefineWorkflowInputViewModel WorkflowInputDefinition { get; set; }

        public async Task OnGetAsync(string workflowDefinitionId)
        {
            var workflowDefinitionSummary = await _workflowDefinitionAppService.GetByDefinitionIdAsync(workflowDefinitionId);
            WorkflowInputDefinition = new DefineWorkflowInputViewModel
            {
                Id = workflowDefinitionSummary.InputDefinition?.Id,
                WorkflowDefinitionId = workflowDefinitionId
            };
            if (workflowDefinitionSummary.InputDefinition == null)
            {
                WorkflowInputDefinition.PropertyDefinitionViewModels.Add(new WorkflowCustomInputPropertyDefinitionViewModel
                {
                    Name = "",
                    Type = WorkflowInputDefinitionProperyType.Text
                });
            }
            else
            {
                WorkflowInputDefinition = ObjectMapper.Map<WorkflowCustomInputDefinitionDto, DefineWorkflowInputViewModel>(workflowDefinitionSummary.InputDefinition);
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _workflowDefinitionAppService.SaveWorkflowInputDefinitionAsync(
                ObjectMapper.Map<DefineWorkflowInputViewModel, WorkflowCustomInputDefinitionDto>(WorkflowInputDefinition)
            );

            return NoContent();
        }
    }
}
