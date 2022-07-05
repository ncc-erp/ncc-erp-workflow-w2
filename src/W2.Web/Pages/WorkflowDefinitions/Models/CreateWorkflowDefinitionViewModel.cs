using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace W2.Web.Pages.WorkflowDefinitions.Models
{
    public class CreateWorkflowDefinitionViewModel
    {
        [Required]
        [Display(Name = "WorkflowDefinition:Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "WorkflowDefinition:DisplayName")]
        public string DisplayName { get; set; }

        public string Tag { get; set; }
    }
}
