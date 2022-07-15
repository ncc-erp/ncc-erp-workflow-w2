using System.ComponentModel.DataAnnotations;

namespace W2.WorkflowDefinitions
{
    public enum WorkflowInputDefinitionProperyType
    {
        [Display(Name = "Text")]
        Text,
        [Display(Name = "Numeric")]
        Numeric,
        [Display(Name = "Date Time")]
        DateTime,
        [Display(Name = "Rich Text")]
        RichText,
        [Display(Name = "User List")]
        UserList
    }
}
