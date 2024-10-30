using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Identity;
using W2.Roles;
using System.Text.Json;
using System;
using JetBrains.Annotations;

namespace W2.Identity
{
    public class W2CustomIdentityRole : IdentityRole
    {
        [Column(TypeName = "jsonb")]
        public string Permissions { get; protected internal set; }
        public DateTime CreationTime { get; protected set; }
        public DateTime? LastModificationTime { get; protected set; }
        public virtual ICollection<W2CustomIdentityUserRole> UserRoles { get; protected set; }        

        protected W2CustomIdentityRole()
        {
        }

        public W2CustomIdentityRole(
            Guid id,
            [NotNull] string name,
            Guid? tenantId = null
        ): base (id, name, tenantId)
        {
        }

        public void SetName(string name)
        {
            Name = name;
            NormalizedName = name.ToUpperInvariant();
        }

        public void SetLastModificationTime(DateTime lastModificationTime)
        {
            LastModificationTime = lastModificationTime;
        }

        [NotMapped]
        public List<PermissionDetailDto> PermissionDtos
        {
            get
            {
                return string.IsNullOrEmpty(Permissions)
                    ? new List<PermissionDetailDto>()
                    : JsonSerializer.Deserialize<List<PermissionDetailDto>>(Permissions);
            }
            set
            {
                Permissions = JsonSerializer.Serialize(value);
            }
        }
    }
}