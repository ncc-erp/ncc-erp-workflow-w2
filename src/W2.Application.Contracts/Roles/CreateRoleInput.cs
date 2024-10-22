using System.Collections.Generic;

namespace W2.Roles
{
    public class CreateRoleInput
    {
        public string Name { get; protected set; }
        public List<string> PermissionCodes { get; protected set; }
    }
}
