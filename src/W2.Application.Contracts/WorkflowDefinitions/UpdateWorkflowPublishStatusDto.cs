using System;
using System.Collections.Generic;
using System.Text;

namespace W2.WorkflowDefinitions
{
    public class UpdateWorkflowPublishStatusDto
    {
        public string WorkflowId { get; set; }
        public bool IsPublished { get; set; }
    }
}
