using System;
using Volo.Abp.Auditing;

namespace W2.WorkflowInstances
{
    public class WorkflowInstanceDto : IMayHaveCreator
    {
        public string Id { get; set; }
        public string WorkflowDefinitionId { get; set; }
        public string WorkflowDefinitionDisplayName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastExecutedAt { get; set; }
        public string Status { get; set; }
        public Guid? CreatorId { get; set; }
    }
}
