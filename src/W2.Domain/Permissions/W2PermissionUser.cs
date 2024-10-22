using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Identity;

namespace W2.Permissions
{
    public class W2PermissionUser : CreationAuditedEntity<Guid>, IMultiTenant
    {
        public Guid PermissionId { get; protected set; }
        public Guid UserId { get; protected set; }
        public Guid? TenantId { get; set; }

        public virtual W2Permission Permission { get; protected set; }
        public virtual IdentityUser User { get; protected set; }
    }
}
