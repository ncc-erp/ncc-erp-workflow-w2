using System;
using System.Collections.Generic;

namespace W2.Roles
{
    public class PermissionDetailDto: PermissionDto
    {
        public List<PermissionDto> Children { get; set; }

        public PermissionDetailDto()
        {
        }

        public PermissionDetailDto(
            Guid id,
            string name,
            string code,
            DateTime creationTime,
            List<PermissionDto> children
        ) : base(id, name, code, creationTime)
        {
            Children = children;
        }
    }
}
