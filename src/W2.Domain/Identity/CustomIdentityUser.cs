using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Identity;
using JetBrains.Annotations;
using System.Collections.Generic;
using W2.Roles;
using System.Text.Json;

namespace W2.Identity
{
    public class W2CustomIdentityUser : IdentityUser
    {
        [Column(TypeName = "jsonb")]
        public string CustomPermissions { get; protected internal set; }

        public W2CustomIdentityUser(
            Guid id,
            [NotNull] string userName,
            [NotNull] string email,            
            Guid? tenantId = null
        ) : base(id, userName, email, tenantId)
        {
        }

        public void SetUsetName(string userName)
        {
            UserName = userName;
        }

        public void SetEmail(string email)
        {
            Email = email;
        }

        public void SetPhoneNumber(string phoneNumber)
        {
            PhoneNumber = phoneNumber;
        }

        public void SetLockoutEnabled(bool lockoutEnabled)
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