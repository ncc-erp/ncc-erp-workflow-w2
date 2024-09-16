using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace W2.WorkflowInstances
{
    public class WFHHistory : CreationAuditedEntity<Guid>
    {
        public string WorkflowInstanceId { get; set; } = default!;
        public string WorkflowDefinitionId { get; set; } = default!;
        public string Email { get; set; } // yyyyMMdd
        public string Branch { get; set; } // yyyyMMdd
        public Int64 RemoteDate { get; set; } // yyyyMMdd
        public Guid? RequestUser { get; set; }
        public Guid? WorkflowInstanceStarterId { get; set; }
    }
}
