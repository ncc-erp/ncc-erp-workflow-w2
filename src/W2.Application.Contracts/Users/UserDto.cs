using System;
using System.Collections.Generic;
using W2.Roles;

namespace W2.Users
{
    public class UserDto
    {
        public Guid? TenantId { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool IsActive { get; set; }
        public bool LockoutEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public string ConcurrencyStamp { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? DeleterId { get; set; }
        public DateTimeOffset? DeletionTime { get; set; }
        public DateTimeOffset LastModificationTime { get; set; }
        public Guid LastModifierId { get; set; }
        public DateTimeOffset CreationTime { get; set; }
        public Guid? CreatorId { get; set; }
        public Guid Id { get; set; }
        public Dictionary<string, object> ExtraProperties { get; set; }
        public List<string> Roles { get; set; }
    }
}
