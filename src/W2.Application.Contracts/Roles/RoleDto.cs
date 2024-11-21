using System;

namespace W2.Roles
{
    public class RoleDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? LastModificationTime { get; set; }
    }
}
