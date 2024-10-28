using System.Collections.Generic;
using System;
using Volo.Abp.Identity;

namespace W2.Identity
{
    public class UpdateUserInput: IdentityUser
    {
        public List<string> RoleNames { get; set; }
        public List<string> CustomPermissionCodes { get; set; }
    }
}
