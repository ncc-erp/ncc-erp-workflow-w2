using System;
using System.Collections.Generic;
using System.Text;

namespace W2.WorkflowInstances
{
    public class WFHRequestDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Branch {  get; set; }
        public long RemoteDate {  get; set; }
        public DateTime CreationTime { get; set; }
        public string Reason { get; set; }
        public WorkflowInstancesStatus Status { get; set; }
    }
}
