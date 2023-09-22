using System;
using System.Collections.Generic;
using System.Text;
using W2.Tasks;

namespace W2.Tasks
{
    public class ListTaskstInput
    {
        public int MaxResultCount { get; set; }
        public int SkipCount { get; set; }
        public string WorkflowDefinitionId { get; set; }
        public W2TaskStatus? Status { get; set; }
        public string KeySearch { get; set; }
        // public string Dates { get; set; }
    }
}
