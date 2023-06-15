using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using W2.WorkflowInstances;

namespace W2.WorkflowInstanceStates
{
    public class WorkflowInstanceState : CreationAuditedEntity<Guid>, IMultiTenant
    {
        public string StateName { get; set; }

        public Guid WorkflowInstanceStarterId { get; set; }
        public WorkflowInstanceStarter WorkflowInstanceStarter { get; set; }

        public Guid? TenantId { get; set; }

        public ICollection<WorkflowInstanceStakeHolder> StakeHolders { get; set; }
    }
}
