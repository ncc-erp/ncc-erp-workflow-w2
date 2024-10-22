using System.Collections.Generic;

namespace W2.Roles
{
    public class CreateRoleInput
    {
        public string Name { get; set; }
        public List<string> PermissionNames { get; set; }
    }
}
