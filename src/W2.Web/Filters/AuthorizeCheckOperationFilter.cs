using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace W2.Web.Filters
{
    public class AuthorizeCheckOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var hasAllowAnonymous = context.MethodInfo.GetCustomAttributes(true)
                .OfType<AllowAnonymousAttribute>().Any()
                || context.MethodInfo.DeclaringType.GetCustomAttributes(true)
                .OfType<AllowAnonymousAttribute>().Any();

            if (hasAllowAnonymous)
            {
                // no auth required
                return;
            }

            // detect Authorize or custom RequirePermission (by name)
            var hasAuthorize = context.MethodInfo.GetCustomAttributes(true).Any(a => a.GetType().Name == "AuthorizeAttribute")
                               || context.MethodInfo.DeclaringType.GetCustomAttributes(true).Any(a => a.GetType().Name == "AuthorizeAttribute");
            var hasRequirePermission = context.MethodInfo.GetCustomAttributes(true).Any(a => a.GetType().Name == "RequirePermissionAttribute")
                                       || context.MethodInfo.DeclaringType.GetCustomAttributes(true).Any(a => a.GetType().Name == "RequirePermissionAttribute");

            if (hasAuthorize || hasRequirePermission)
            {
                // Add security requirement for this operation
                operation.Security ??= new System.Collections.Generic.List<OpenApiSecurityRequirement>();

                var scheme = new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                };

                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    [scheme] = new string[] { }
                });
            }
        }
    }
}