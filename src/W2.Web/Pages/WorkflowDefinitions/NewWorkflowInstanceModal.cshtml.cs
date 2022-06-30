using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using W2.Web.Pages.WorkflowDefinitions.Models;
using W2.WorkflowDefinitions;

namespace W2.Web.Pages.WorkflowDefinitions
{
    public class NewWorkflowInstanceModalModel : W2PageModel
    {
        private readonly IWorkflowDefinitionAppService _workflowDefinitionAppService;

        public NewWorkflowInstanceModalModel(IWorkflowDefinitionAppService workflowDefinitionAppService)
        {
            _workflowDefinitionAppService = workflowDefinitionAppService;
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
            await _workflowDefinitionAppService.CreateNewInstanceAsync(new ExecuteWorkflowDefinitionDto
            {
                WorkflowDefinitionId = WorkflowDefinitionId,
                Input = WorkflowInput
            });

            return NoContent();
        }
    }
}
