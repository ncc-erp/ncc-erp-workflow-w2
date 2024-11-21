using System.Collections.Generic;
using W2.Users;

namespace W2.Roles
{
    public class RoleDetailDto: RoleDto
    {
        public List<PermissionDetailDto> Permissions { get; set; }
        public List<UserDto> Users { get; set; }
    }
}
