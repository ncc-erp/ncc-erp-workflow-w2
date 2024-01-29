using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using Volo.Abp.Security.Claims;
using Volo.Abp.Users;
using W2.Permissions;

namespace W2.Web.Pages.CompOnly.Designer
{
    [Authorize(W2Permissions.WorkflowManagementWorkflowDefinitionsDesign)]
    public class IndexModel : W2PageModel
    {
        [BindProperty(SupportsGet = true)]
        [FromQuery(Name = "id")]
        public string WorkflowDefinitionId { get; set; }
        public void OnGet()
        {
        }
    }
}