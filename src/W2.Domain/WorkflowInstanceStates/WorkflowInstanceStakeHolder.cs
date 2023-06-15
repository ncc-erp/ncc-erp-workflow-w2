using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;

namespace W2.WorkflowInstanceStates
{
    public class WorkflowInstanceStakeHolder : CreationAuditedEntity, IMultiTenant
    {
        public Guid WorkflowInstanceStateId { get; set; }
        public WorkflowInstanceState WorkflowInstanceState { get; set; }

        public Guid UserId { get; set; }
        public IdentityUser User { get; set; }

        public Guid? TenantId { get; set; }

        public override object[] GetKeys()
        {
            return new object[] { WorkflowInstanceStateId, UserId };
        }
    }
}
