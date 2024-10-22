using System.Collections.Generic;

namespace W2.Roles
{
    public class PermissionDetailDto: PermissionDto
    {
        public List<PermissionDto> Children { get; set; }
    }
}
