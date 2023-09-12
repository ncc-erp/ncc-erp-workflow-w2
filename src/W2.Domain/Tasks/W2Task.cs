using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace W2.Tasks
{
    public class W2Task : CreationAuditedEntity<Guid>, IMultiTenant
    {
        //public string WorkflowInstanceId { get; set; } = default!;
        //public Dictionary<string, string> Input { get; set; }
        public string WorkflowInstanceId { get; set; } = default!;
        public string Email { get; set; }
        public Dictionary<string, string> Data { get; set; }
        public Guid? TenantId { get; set; }
        public Guid Author { get; set; }
        public W2TaskStatus Status { get; set; }
    }
}
