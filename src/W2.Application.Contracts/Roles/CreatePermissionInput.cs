using JetBrains.Annotations;
using System;

namespace W2.Roles
{
    public class CreatePermissionInput
    {
        public string Name { get; set; }
        public string Code { get; set; }
        [CanBeNull]
        public Guid ParentId { get; set; }
    }
}
