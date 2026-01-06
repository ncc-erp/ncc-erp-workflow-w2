using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace W2.WorkflowInstances
{
    public class W2RequestHistory : CreationAuditedEntity<Guid>, IMultiTenant
    {
        public string WorkflowInstanceId { get; set; } = default!;
        public string WorkflowDefinitionId { get; set; } = default!;
        public Guid WorkflowInstanceStarterId { get; set; }
        
        public string Email { get; set; } = default!;
        public DateTime Date { get; set; }
        public WorkflowInstancesStatus Status { get; set; }
        public string RequestType { get; set; } = default!;
        
        public Guid? TenantId { get; set; }
    }
}
