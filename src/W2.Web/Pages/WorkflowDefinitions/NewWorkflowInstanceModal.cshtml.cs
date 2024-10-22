using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using W2.Web.Pages.WorkflowDefinitions.Models;
using W2.WorkflowDefinitions;
using W2.WorkflowInstances;
using W2.Permissions;
using Microsoft.AspNetCore.Authorization;
using W2.ExternalResources;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace W2.Web.Pages.WorkflowDefinitions
{
    [Authorize(W2Permissions.WorkflowManagementWorkflowInstancesCreate)]
    public class NewWorkflowInstanceModalModel : W2PageModel
    {
        private readonly IWorkflowInstanceAppService _workflowInstanceAppService;
        private readonly IExternalResourceAppService _externalResourceAppService;
        private readonly IWorkflowDefinitionAppService _workflowDefinitionAppService;
        private readonly ILogger<NewWorkflowInstanceModalModel> _logger;

        public NewWorkflowInstanceModalModel(IWorkflowInstanceAppService workflowInstanceAppService, 
            IExternalResourceAppService externalResourceAppService,
            IWorkflowDefinitionAppService workflowDefinitionAppService,
            ILogger<NewWorkflowInstanceModalModel> logger)
        {
            _workflowDefinitionAppService = workflowDefinitionAppService;
            _workflowInstanceAppService = workflowInstanceAppService;
            _externalResourceAppService = externalResourceAppService;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public string WorkflowDefinitionId { get; set; }
        [BindProperty(SupportsGet = true)]
        public string PropertiesDefinitionJson { get; set; }
        public List<WorkflowCustomInputPropertyDefinitionViewModel> PropertyDefinitionViewModels { get; set; }
        [BindProperty]
        [FromForm]
        public Dictionary<string, string> WorkflowInput { get; set; } = new Dictionary<string, string>();
        public List<SelectListItem> UserSelectListItems { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ProjectSelectListItems { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> OfficeSelectListItems { get; set; } = new List<SelectListItem>();
        public OfficeInfo CurrentOffice { get; set; }
        public ProjectProjectItem CurrentProject { get; set; }
        public WorkflowDefinitionSummaryDto WorkflowDefinitionSummaryDto { get; set; }
        public async Task OnGetAsync()
        {
            WorkflowDefinitionSummaryDto = await _workflowDefinitionAppService.GetByDefinitionIdAsync(WorkflowDefinitionId);

            CurrentOffice = await _externalResourceAppService.GetUserBranchInfoAsync(CurrentUser.Email);
            CurrentProject = await _externalResourceAppService.GetCurrentUserWorkingProjectAsync();

            PropertyDefinitionViewModels = JsonConvert.DeserializeObject<List<WorkflowCustomInputPropertyDefinitionViewModel>>(PropertiesDefinitionJson);
            foreach (var propertyDefinition in PropertyDefinitionViewModels)
            {
                WorkflowInput.Add(propertyDefinition.Name, null);
            }

            if (PropertyDefinitionViewModels.Any(x => x.Type == WorkflowInputDefinitionProperyType.UserList))
            {
                var users = await _externalResourceAppService.GetAllUsersInfoAsync();
                UserSelectListItems = users
                    .Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Email
                    })
                    .ToList();
            }            
            
            if (PropertyDefinitionViewModels.Any(x => x.Type == WorkflowInputDefinitionProperyType.MyProject))
            {
                var projects = await _externalResourceAppService.GetCurrentUserProjectsAsync();
                ProjectSelectListItems = projects
                    .Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Code,
                        Selected = x.Code == CurrentProject?.Code
                    })
                    .ToList();
            }

            if (PropertyDefinitionViewModels.Any(x => x.Type == WorkflowInputDefinitionProperyType.MyPMProject))
            {
                var projects = await _externalResourceAppService.GetUserProjectsWithRolePMFromApiAsync();
                ProjectSelectListItems = projects
                    .Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Code,
                        Selected = x.Code == CurrentProject?.Code
                    })
                    .ToList();
            }

            if (PropertyDefinitionViewModels.Any(x => x.Type == WorkflowInputDefinitionProperyType.OfficeList))
            {
                var offices = await _externalResourceAppService.GetListOfOfficeAsync();
                OfficeSelectListItems = offices
                    .Select(x => new SelectListItem
                    {
                        Text = x.DisplayName,
                        Value = x.Code,
                        Selected = x.Code == CurrentOffice?.Code
                    })
                    .ToList();
            }
        }

        //public async Task<IActionResult> OnPostAsync()
        //{
        //    var workflowInstance = await _workflowInstanceAppService.CreateNewInstanceAsync(new CreateNewWorkflowInstanceDto
        //    {
        //        WorkflowDefinitionId = WorkflowDefinitionId,
        //        Input = WorkflowInput
        //    });

        //    _logger.LogInformation($"Return Id {workflowInstance.id} to client");
        //    return Content(workflowInstance.id);
        //}
    }
}
