using System;
using Volo.Abp.Identity;

namespace W2.Identity
{
    public class W2CustomIdentityUserRole : IdentityUserRole
    {
        public virtual W2CustomIdentityUser User { get; set; }
        public virtual W2CustomIdentityRole Role { get; set; }

        protected W2CustomIdentityUserRole()
        {
        }

        public W2CustomIdentityUserRole(
            Guid userId,
            Guid roleId,
            Guid? tenantId = null
        ) : base(userId, roleId, tenantId)
        {
        }
    }
}