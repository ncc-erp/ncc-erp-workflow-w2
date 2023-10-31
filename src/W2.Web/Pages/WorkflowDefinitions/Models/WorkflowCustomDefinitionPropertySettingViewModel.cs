using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace W2.Web.Pages.WorkflowDefinitions.Models
{
    public class WorkflowCustomDefinitionPropertySettingViewModel
    {
        [Required]
        [DisplayName("Key Setting")]
        public string Key { get; set; }

        [DisplayName("Value Setting")]
        public string Value { get; set; }
    }
}
