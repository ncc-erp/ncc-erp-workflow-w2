using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace W2.WorkflowInstances
{
    public class ListAllWFHRequestInput
    {
        public int MaxResultCount { get; set; }
        public int SkipCount { get; set; }
        public string Sorting { get; set; }
        public string KeySearch { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public WorkflowInstancesStatus Status { get; set; } = WorkflowInstancesStatus.Approved;
    }
}
