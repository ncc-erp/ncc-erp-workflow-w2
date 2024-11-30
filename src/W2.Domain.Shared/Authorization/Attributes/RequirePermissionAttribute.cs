using Microsoft.AspNetCore.Authorization;
using System;

namespace W2.Authorization.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permission)
        : base(permission)
    {
        Policy = permission;
    }
}
