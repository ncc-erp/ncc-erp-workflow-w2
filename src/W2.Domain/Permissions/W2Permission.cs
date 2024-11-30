using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using W2.Roles;

namespace W2.Permissions
{
    public class W2Permission : CreationAuditedEntity<Guid>, IMultiTenant
    {
        public string Name { get; protected set; }
        public string Code { get; protected set; }
        public Guid? ParentId { get; protected set; }
        public Guid? TenantId { get; protected set; }

        public virtual W2Permission Parent { get; protected set; }
        public virtual ICollection<W2Permission> Children { get; protected set; }

        public W2Permission(
            string name,
            string code,
            Guid? parentId = null,
            Guid? tenantId = null
        )
        {
            Name = name;
            Code = code;
            ParentId = parentId;
            TenantId = tenantId;
        }
        public void SetName(string name)
        {
            Name = name;
        }

        public void SetCode(string code)
        {
            Code = code;
        }

        public void SetParentId(Guid? parentId)
        {
            ParentId = parentId;
        }

        public void SetTenantId(Guid? tenantId)
        {
            TenantId = tenantId;
        }

        public void SetId(Guid id)
        {
            Id = id;
        }

        public static List<PermissionDetailDto> BuildPermissionHierarchy(List<W2Permission> permissions)
        {
            return permissions
                .Where(p => p.ParentId == null)
                .Select(parent =>
                {
                    var childrenDto = permissions
                        .Where(c => c.ParentId == parent.Id)
                        .Select(child => new PermissionDto(
                            child.Id, child.Name, child.Code, child.CreationTime
                        ))
                        .ToList();
                    var parentDto = new PermissionDetailDto(
                        parent.Id, parent.Name, parent.Code, parent.CreationTime, childrenDto
                    );
                    return parentDto;
                })
                .ToList();
        }

        public static List<string> GetPermissionCodes(List<PermissionDetailDto> permissions)
        {
            if (permissions == null) return new List<string>();

            var permissionCodes = new List<string>();

            foreach (var permission in permissions)
            {
                permissionCodes.Add(permission.Code);
                foreach (var child in permission.Children)
                {
                    permissionCodes.Add(child.Code);
                }
            }

            return permissionCodes;
        }
    }
}
