using System.Collections.Generic;
using System;
using Volo.Abp.Identity;

namespace W2.Identity
{
    public class UpdateUserInput: IdentityUser
    {
        public List<Guid> CustomPermissionIds { get; set; }
    }
}
