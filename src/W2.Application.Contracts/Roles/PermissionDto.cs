using System;

namespace W2.Roles
{
    public class PermissionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public DateTime CreationTime { get; set; }

        public PermissionDto()
        {
        }

        public PermissionDto(
            Guid id,
            string name,
            string code,
            DateTime creationTime
        )
        {
            Id = id;
            Name = name;
            Code = code;
            CreationTime = creationTime;
        }
    }
}
