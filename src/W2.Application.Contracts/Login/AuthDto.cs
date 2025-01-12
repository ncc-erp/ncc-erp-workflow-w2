using System;
using System.Collections.Generic;
using System.Text;

namespace W2.Login
{
    public class AuthDto
    {
        public string userNameOrEmailAddress { get; set; }
        public string password { get; set; }
        public bool? rememberClient { get; set; }
    }

    public class AuthUser
    {
        public string Token { get; set; }
    }

    public class UserInfo
    {
        public string[] sub { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string given_name { get; set; }
        public string role { get; set; }
        public string[] permissions { get; set; }
    }
}
