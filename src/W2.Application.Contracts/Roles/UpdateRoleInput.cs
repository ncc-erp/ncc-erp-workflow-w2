using System.Collections.Generic;

namespace W2.Roles
{
    public class UpdateRoleInput
    {
        public string Name { get; protected set; }
        public List<string> PermissionCodes { get; protected set; }
    }
}
