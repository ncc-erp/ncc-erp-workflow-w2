using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<DefineWorkflowInputModalModel> _logger;

        public DefineWorkflowInputModalModel(IWorkflowDefinitionAppService workflowDefinitionAppService, 
            ILogger<DefineWorkflowInputModalModel> logger)
        {
            _workflowDefinitionAppService = workflowDefinitionAppService;
            _logger = logger;
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
                    Type = WorkflowInputDefinitionProperyType.Text,
                    IsRequired = false,
                    IsTitle = false
                });
            }
            else
            {
                try
                {
                    WorkflowInputDefinition = ObjectMapper.Map<WorkflowCustomInputDefinitionDto, DefineWorkflowInputViewModel>(workflowDefinitionSummary.InputDefinition);
                }
                catch (System.Exception ex)
                {
                    WorkflowInputDefinition.PropertyDefinitionViewModels.Add(new WorkflowCustomInputPropertyDefinitionViewModel
                    {
                        Name = "",
                        Type = WorkflowInputDefinitionProperyType.Text,
                        IsRequired = false,
                        IsTitle = false

                    });
                    _logger.LogException(ex);
                }
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
