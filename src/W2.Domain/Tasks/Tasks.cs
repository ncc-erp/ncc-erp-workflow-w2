using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace W2.Tasks
{
    public class MyTask : CreationAuditedEntity<Guid>
    {
        public string Email { get; set; }
        public string Status { get; set; }

        public string WorkflowInstanceId { get; set; } = default!;
    }
}
