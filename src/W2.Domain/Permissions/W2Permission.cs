using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace W2.Permissions
{
    public class W2Permission : CreationAuditedEntity<Guid>, IMultiTenant
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public Guid? ParentId { get; set; }
        public Guid? TenantId { get; set; }

        public virtual ICollection<W2PermissionRole> PermissionRoles { get; protected set; }
        public virtual ICollection<W2PermissionUser> PermissionUsers { get; protected set; }
    }
}
