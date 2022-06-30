using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace W2.WorkflowInstances
{
    public class WorkflowInstanceStarter : CreationAuditedAggregateRoot<Guid>, IMultiTenant
    {
        public string WorkflowDefinitionId { get; set; } = default!;
        public Guid? TenantId { get; set; }
        public Dictionary<string, object> Properties { get; set; }
    }
}
