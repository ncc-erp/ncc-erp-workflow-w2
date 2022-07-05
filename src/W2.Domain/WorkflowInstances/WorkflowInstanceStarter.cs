using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace W2.WorkflowInstances
{
    public class WorkflowInstanceStarter : CreationAuditedAggregateRoot<Guid>, IMultiTenant
    {
        public string WorkflowInstanceId { get; set; } = default!;
        public Guid? TenantId { get; set; }
    }
}
