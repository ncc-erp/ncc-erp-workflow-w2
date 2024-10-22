using System.Collections.Generic;

namespace W2.Roles
{
    public class UpdateRoleInput
    {
        public string Name { get; set; }
        public List<string> PermissionCodes { get; set; }
    }
}
