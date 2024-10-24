using System.Collections.Generic;
using Volo.Abp.Identity;

namespace W2.Roles
{
    public class RoleDetailDto: IdentityRoleDto
    {
        public List<PermissionDetailDto> Permissions { get; set; }
    }
}
