using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace W2.Permissions
{
    public class W2Permission : CreationAuditedEntity<Guid>, IMultiTenant
    {
        public string Name { get; protected set; }
        public string Code { get; protected set; }
        public Guid? ParentId { get; protected set; }
        public Guid? TenantId { get; protected set; }

        public virtual ICollection<W2PermissionRole> PermissionRoles { get; protected set; }
        public virtual ICollection<W2PermissionUser> PermissionUsers { get; protected set; }

        public W2Permission(string name, string code, Guid? parentId, Guid? tenantId) 
        {
            Name = name;
            Code = code;
            ParentId = parentId;
            TenantId = tenantId;
        }
    }
}
