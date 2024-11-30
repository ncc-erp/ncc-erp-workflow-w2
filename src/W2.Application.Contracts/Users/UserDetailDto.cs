using System;
using System.Collections.Generic;
using W2.Roles;

namespace W2.Users
{
    public class UserDetailDto : UserDto
    {
        public List<string> CustomPermissions { get; set; }
    }
}
