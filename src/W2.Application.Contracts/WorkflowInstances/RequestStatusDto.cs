using System;

namespace W2.WorkflowInstances
{
    public class RequestStatusDto
    {
        public string Email { get; set; }
        public DateTime Date { get; set; }
        public WorkflowInstancesStatus Status { get; set; }
        public string Type { get; set; }
    }
}