using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace W2.WorkflowDefinitions
{
    public class ExecuteWorkflowDefinitionDto
    {
        [Required]
        public string WorkflowDefinitionId { get; set; }
        public object Input { get; set; }
    }
}
