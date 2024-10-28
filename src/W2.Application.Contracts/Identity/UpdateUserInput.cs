using System.Collections.Generic;
using Volo.Abp.Identity;

namespace W2.Identity
{
    public class UpdateUserInput: IdentityUserUpdateDto
    {
        public List<string> CustomPermissionCodes { get; set; }
    }
}
