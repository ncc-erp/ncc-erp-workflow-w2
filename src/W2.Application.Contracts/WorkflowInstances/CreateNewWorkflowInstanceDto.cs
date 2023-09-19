using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace W2.WorkflowInstances
{
    public class CreateNewWorkflowInstanceDto
    {
        [Required]
        public string WorkflowDefinitionId { get; set; }
        public Dictionary<string, string> Input { get; set; }
    }
}
