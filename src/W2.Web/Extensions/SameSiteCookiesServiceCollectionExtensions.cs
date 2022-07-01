using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace W2.Web.Extensions
{
    public static class SameSiteCookiesServiceCollectionExtensions
    {
        public static IServiceCollection AddSameSiteCookiePolicy(this IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                options.OnAppendCookie = cookieContext =>
                    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
                options.OnDeleteCookie = cookieContext =>
                    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
            });

            return services;
        }

        private static void CheckSameSite(HttpContext httpContext, CookieOptions options)
        {
            if (httpContext.Request.IsHttps)
            {
                options.SameSite = SameSiteMode.None;
                options.Secure = true;
            }
            else if (options.SameSite == SameSiteMode.None)
            {
                options.SameSite = SameSiteMode.Unspecified;
            }
        }
    }
}
