using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Identity;

namespace W2.Permissions
{
    public class W2PermissionRole : CreationAuditedEntity<Guid>, IMultiTenant
    {
        public Guid PermissionId { get; protected set; }
        public Guid RoleId { get; protected set; }
        public Guid? TenantId { get; protected set; }

        public virtual W2Permission Permission { get; protected set; }
        public virtual IdentityRole Role { get; protected set; }

        public W2PermissionRole(Guid permissionId, Guid roleId, Guid? tenantId)
        {
            PermissionId = permissionId;
            RoleId = roleId;
            TenantId = tenantId;
        }
    }
}
