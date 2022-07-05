using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using W2.Web.Pages.WorkflowDefinitions.Models;
using W2.WorkflowDefinitions;
using W2.WorkflowInstances;
using W2.Permissions;
using Microsoft.AspNetCore.Authorization;

namespace W2.Web.Pages.WorkflowDefinitions
{
    [Authorize(W2Permissions.WorkflowManagementWorkflowInstancesCreate)]
    public class NewWorkflowInstanceModalModel : W2PageModel
    {
        private readonly IWorkflowInstanceAppService _workflowInstanceAppService;

        public NewWorkflowInstanceModalModel(IWorkflowInstanceAppService workflowInstanceAppService)
        {
            _workflowInstanceAppService = workflowInstanceAppService;
        }

        [BindProperty(SupportsGet = true)]
        public string WorkflowDefinitionId { get; set; }
        [BindProperty(SupportsGet = true)]
        public string PropertiesDefinitionJson { get; set; }
        public List<WorkflowCustomInputPropertyDefinitionViewModel> PropertyDefinitionViewModels { get; set; }
        [BindProperty]
        [FromForm]
        public Dictionary<string, string> WorkflowInput { get; set; } = new Dictionary<string, string>();

        public void OnGet()
        {
            PropertyDefinitionViewModels = JsonConvert.DeserializeObject<List<WorkflowCustomInputPropertyDefinitionViewModel>>(PropertiesDefinitionJson);
            foreach (var propertyDefinition in PropertyDefinitionViewModels)
            {
                WorkflowInput.Add(propertyDefinition.Name, null);
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var workflowInstanceId = await _workflowInstanceAppService.CreateNewInstanceAsync(new CreateNewWorkflowInstanceDto
            {
                WorkflowDefinitionId = WorkflowDefinitionId,
                Input = WorkflowInput
            });

            return Content(workflowInstanceId);
        }
    }
}
