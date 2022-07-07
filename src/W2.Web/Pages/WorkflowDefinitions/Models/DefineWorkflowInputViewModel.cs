using System;
using System.Collections.Generic;

namespace W2.Web.Pages.WorkflowDefinitions.Models
{
    public class DefineWorkflowInputViewModel
    {
        public Guid? Id { get; set; }
        public string WorkflowDefinitionId { get; set; }
        public List<WorkflowCustomInputPropertyDefinitionViewModel> PropertyDefinitionViewModels { get; set; } = new List<WorkflowCustomInputPropertyDefinitionViewModel>();
    }
}
