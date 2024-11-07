using System.Collections.Generic;

namespace W2.Roles
{
    public class RoleDetailDto: RoleDto
    {
        public List<PermissionDetailDto> Permissions { get; set; }
    }
}
