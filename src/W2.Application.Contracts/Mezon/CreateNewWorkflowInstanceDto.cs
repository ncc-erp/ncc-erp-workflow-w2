using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace W2.Mezon;

public class CreateNewWorkflowInstanceDto
{
    [Required]
    public string WorkflowDefinitionId { get; set; }
    public Dictionary<string, string> DataInputs { get; set; }
    public string Email { get; set; }
}