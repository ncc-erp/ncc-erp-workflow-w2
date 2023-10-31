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
    public class DefineWorkflowSettingModalModel : W2PageModel
    {
        private readonly IWorkflowDefinitionAppService _workflowDefinitionAppService;
        private readonly ILogger<DefineWorkflowSettingModalModel> _logger;

        public DefineWorkflowSettingModalModel(IWorkflowDefinitionAppService workflowDefinitionAppService, 
            ILogger<DefineWorkflowSettingModalModel> logger)
        {
            _workflowDefinitionAppService = workflowDefinitionAppService;
            _logger = logger;
        }

        [BindProperty]
        public DefineWorkflowSettingViewModel WorkflowSettingDefinition { get; set; }

        public async Task OnGetAsync(string workflowDefinitionId)
        {
            var workflowDefinitionSummary = await _workflowDefinitionAppService.GetByDefinitionIdAsync(workflowDefinitionId);
            WorkflowSettingDefinition = new DefineWorkflowSettingViewModel
            {
                Id = workflowDefinitionSummary.InputDefinition?.Id,
                WorkflowDefinitionId = workflowDefinitionId
            };
            if (workflowDefinitionSummary.InputDefinition == null)
            {
                WorkflowSettingDefinition.PropertyDefinitionViewModels.Add(new WorkflowCustomDefinitionPropertySettingViewModel
                {
                    Key = "",
                    Value=""
                });
            }
            else
            {
                try
                {
                    WorkflowSettingDefinition = ObjectMapper.Map<WorkflowCustomDefinitionSettingDto, DefineWorkflowSettingViewModel>(workflowDefinitionSummary.SettingDefinition);
                }
                catch (System.Exception ex)
                {
                WorkflowSettingDefinition.PropertyDefinitionViewModels.Add(new WorkflowCustomDefinitionPropertySettingViewModel
                {
                    Key = "",
                    Value=""
                });
                    _logger.LogException(ex);
                }
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _workflowDefinitionAppService.SaveWorkflowDefinitionSettingAsync(
                ObjectMapper.Map<DefineWorkflowSettingViewModel, WorkflowCustomDefinitionSettingDto>(WorkflowSettingDefinition)
            );

            return NoContent();
        }
    }
}
