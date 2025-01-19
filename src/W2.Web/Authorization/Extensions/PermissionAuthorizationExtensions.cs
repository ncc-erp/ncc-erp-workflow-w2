using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using W2.Authorization.Handlers;
using W2.Constants;

namespace W2.Authorization.Extensions;

public static class PermissionAuthorizationExtensions
{
    public static IServiceCollection AddPermissionAuthorization(
        this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, PermissionHandler>();
        services.AddAuthorization(options =>
        {
            var permissions = typeof(W2ApiPermissions)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .ToList()
                .Select(p => p.GetValue(null).ToString());

            foreach (var permission in permissions)
            {
                options.AddPolicy(permission,
                    policy => policy.Requirements.Add(
                        new PermissionRequirement(permission)));
            }
        });

        return services;
    }
}
