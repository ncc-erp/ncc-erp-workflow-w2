using System;
using System.Collections.Generic;
using System.Text;

namespace W2.Permissions
{
    public class UpdatePermissionInput
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public Guid? ParentId { get; set; }
    }
}
