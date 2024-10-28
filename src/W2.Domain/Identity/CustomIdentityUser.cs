using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Identity;
using JetBrains.Annotations;
using System.Collections.Generic;
using W2.Roles;
using System.Text.Json;
using Volo.Abp;

namespace W2.Identity
{
    public class W2CustomIdentityUser : IdentityUser
    {
        [Column(TypeName = "jsonb")]
        public string CustomPermissions { get; protected internal set; }
        public virtual ICollection<W2CustomIdentityUserRole> UserRoles { get; set; }

        protected W2CustomIdentityUser()
        {
        }

        public W2CustomIdentityUser(
            Guid id,
            [NotNull] string userName,
            [NotNull] string email,
            Guid? tenantId = null
        ) : base(id, userName, email, tenantId)
        {
        }

        public virtual void SetUserName(string userName)
        {
            Check.NotNullOrWhiteSpace(userName, nameof(userName));
            UserName = userName;
        }

        public virtual void SetEmail(string email)
        {
            Check.NotNullOrWhiteSpace(email, nameof(email));
            Email = email;
        }

        public virtual void SetPhoneNumber(string phoneNumber)
        {
            PhoneNumber = phoneNumber;
        }

        public virtual void SetLockoutEnabled(bool lockoutEnabled)
        {
            LockoutEnabled = lockoutEnabled;
        }

        [NotMapped]
        public List<PermissionDetailDto> CustomPermissionDtos
        {
            get
            {
                return string.IsNullOrEmpty(CustomPermissions)
                    ? new List<PermissionDetailDto>()
                    : JsonSerializer.Deserialize<List<PermissionDetailDto>>(CustomPermissions);
            }
            set
            {
                CustomPermissions = JsonSerializer.Serialize(value);
            }
        }
    }
}