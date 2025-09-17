using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using W2.EntityFrameworkCore;
using W2.Localization;
using W2.MultiTenancy;
using W2.Web.Menus;
using Microsoft.OpenApi.Models;
using Volo.Abp;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.Authentication.JwtBearer;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Basic;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Basic.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.AutoMapper;
using Volo.Abp.Identity.Web;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.SettingManagement.Web;
using Volo.Abp.Swashbuckle;
using Volo.Abp.TenantManagement.Web;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.UI.Navigation;
using Volo.Abp.VirtualFileSystem;
using Elsa.Persistence.EntityFramework.Core.Extensions;
using Elsa.Activities.UserTask.Extensions;
using Elsa;
using Volo.Abp.AspNetCore.Mvc.AntiForgery;
using Volo.Abp.SettingManagement.Web.Pages.SettingManagement;
using W2.Web.Settings;
using Microsoft.AspNetCore.Identity;
using W2.Identity;
using W2.Web.Extensions;
using Microsoft.AspNetCore.Http;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.MultiTenancy;
using W2.Configurations;
using Microsoft.AspNetCore.Mvc.RazorPages;
using W2.Permissions;
using W2.Activities;
using W2.Scripting;
using Elsa.Persistence.EntityFramework.PostgreSql;
using Volo.Abp.Timing;
using Volo.Abp.IdentityServer;
using Parlot.Fluent;
using System.Linq;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Collections.Generic;
using W2.Web.Workflows;
using Microsoft.AspNetCore.HttpOverrides;
using Autofac.Core;
using Microsoft.EntityFrameworkCore.Internal;
using W2.Authorization.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text.Json;
using System.Threading.Tasks;
using W2.Jobs;
using Volo.Abp.BackgroundWorkers;
using W2.Web.Filters;

namespace W2.Web;
[DependsOn(
    typeof(W2HttpApiModule),
    typeof(W2ApplicationModule),
    typeof(W2EntityFrameworkCoreModule),
    typeof(AbpAutofacModule),
    typeof(AbpIdentityWebModule),
    typeof(AbpSettingManagementWebModule),
    typeof(AbpAccountWebIdentityServerModule),
    typeof(AbpAspNetCoreMvcUiBasicThemeModule),
    typeof(AbpAspNetCoreAuthenticationJwtBearerModule),
    typeof(AbpTenantManagementWebModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpSwashbuckleModule)
    )]
public class W2WebModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.PreConfigure<AbpMvcDataAnnotationsLocalizationOptions>(options =>
        {
            options.AddAssemblyResource(
                typeof(W2Resource),
                typeof(W2DomainModule).Assembly,
                typeof(W2DomainSharedModule).Assembly,
                typeof(W2ApplicationModule).Assembly,
                typeof(W2ApplicationContractsModule).Assembly,
                typeof(W2WebModule).Assembly
            );
        });

        PreConfigure<IdentityBuilder>(identityBuilder =>
        {
            identityBuilder.AddSignInManager<CustomSignInManager>();
            identityBuilder.AddUserManager<CustomUserManager>();
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        ConfigureUrls(configuration);
        ConfigureBundles();
        ConfigureAuthentication(context, configuration);
        ConfigureAutoMapper();
        ConfigureVirtualFileSystem(hostingEnvironment);
        ConfigureLocalizationServices();
        ConfigureNavigationServices();
        ConfigureAutoApiControllers();
        ConfigureSwaggerServices(context.Services);
        ConfigureElsa(context, configuration);
        ConfigureExternalLogins(context, configuration);
        ConfigureMultiTenancy(configuration);
        ConfigureRazorPages();

        context.Services.AddAbpApiVersioning(options =>
        {
            // Show neutral/versionless APIs.
            options.UseApiBehavior = false;

            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
        });

        context.Services.AddPermissionAuthorization();

        Configure<SettingManagementPageOptions>(options =>
        {
            options.Contributors.Add(new SocialLoginSettingsPageContributor());
        });

        context.Services.AddSameSiteCookiePolicy();

        Configure<AbpClockOptions>(options =>
        {
            options.Kind = DateTimeKind.Utc;
        });

        Configure<AbpClaimsServiceOptions>(options =>
        {
            options.RequestedClaims.Add(CustomClaim.ProjectName);
        });
    }

    private void ConfigureUrls(IConfiguration configuration)
    {
        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
        });
    }

    private void ConfigureBundles()
    {
        Configure<AbpBundlingOptions>(options =>
        {
            options.StyleBundles.Configure(
                BasicThemeBundles.Styles.Global,
                bundle =>
                {
                    bundle.AddFiles(
                        "/global-styles.css"
                    );
                }
            );
            options.ScriptBundles.Configure(typeof(IndexModel).FullName,
                configuration =>
                {
                    configuration.AddFiles("/Pages/SettingManagement/Components/SocialLoginSettingGroup/Default.js");
                });
        });
    }

    private void ConfigureAuthentication(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"])),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                    AuthenticationType = "Identity.Application"
                };
                
                options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = OnTokenValidated
                    };
            });
    }

    private Task OnTokenValidated(TokenValidatedContext context)
    {
        if (!HasValidClaims(context))
        {
            context.Fail("401 Unauthorized");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }

        return Task.CompletedTask;
    }
    
    private bool HasValidClaims(TokenValidatedContext context)
    {
        var hasPermissionsClaim = context.Principal != null && context.Principal.HasClaim(claim => claim.Type == "permissions");
        return hasPermissionsClaim;
    }

    private void ConfigureAutoMapper()
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<W2WebModule>();
        });
    }

    private void ConfigureVirtualFileSystem(IWebHostEnvironment hostingEnvironment)
    {
        if (hostingEnvironment.IsDevelopment())
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<W2DomainSharedModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}W2.Domain.Shared"));
                options.FileSets.ReplaceEmbeddedByPhysical<W2DomainModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}W2.Domain"));
                options.FileSets.ReplaceEmbeddedByPhysical<W2ApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}W2.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<W2ApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}W2.Application"));
                options.FileSets.ReplaceEmbeddedByPhysical<W2WebModule>(hostingEnvironment.ContentRootPath);
            });
        }

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<W2WebModule>("W2.Web");
        });
    }

    private void ConfigureLocalizationServices()
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Languages.Add(new LanguageInfo("en", "en", "English"));
            //options.Languages.Add(new LanguageInfo("vi-VN", "vi-VN", "Tiếng Việt"));
        });
    }

    private void ConfigureNavigationServices()
    {
        Configure<AbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new W2MenuContributor());
        });
    }

    private void ConfigureAutoApiControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(W2ApplicationModule).Assembly);
        });
    }

    private void ConfigureSwaggerServices(IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            // Resolve simple conflicts
            c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

            // Doc inclusion predicate: decide per-doc (public / protected) using attributes on MethodInfo or endpoint metadata
            c.DocInclusionPredicate((docName, apiDesc) =>
            {
                if (apiDesc == null)
                    return false;

                // Try endpoint metadata first
                var endpointMetadata = apiDesc.ActionDescriptor?.EndpointMetadata;

                // Determine HttpMethod presence (ApiDescription may already contain)
                var hasHttpMethod = !string.IsNullOrEmpty(apiDesc.HttpMethod);

                // Fallback to inspect ControllerActionDescriptor.MethodInfo (covers ABP conventional controllers)
                var cad = apiDesc.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
                var methodInfo = cad?.MethodInfo;

                if (!hasHttpMethod && methodInfo != null)
                {
                    // Detect attribute-based HTTP method (HttpGet/HttpPost/HttpPut/HttpDelete/HttpPatch or HttpMethodAttribute)
                    hasHttpMethod = methodInfo.GetCustomAttributes(true).Any(a =>
                    {
                        var n = a.GetType().Name;
                        return n.StartsWith("HttpGet") || n.StartsWith("HttpPost") ||
                               n.StartsWith("HttpPut") || n.StartsWith("HttpDelete") ||
                               n.StartsWith("HttpPatch") || n == "HttpMethodAttribute";
                    });

                    // If still unknown, check for Route attribute on declaring type (might indicate routing)
                    if (!hasHttpMethod)
                    {
                        hasHttpMethod = methodInfo.DeclaringType?.GetCustomAttributes(true)
                            .Any(a => a.GetType().Name.Contains("RouteAttribute")) == true;
                    }
                }

                // If we cannot determine HTTP method, skip to avoid ambiguous method errors
                if (!hasHttpMethod)
                {
                    return false;
                }

                // detect AllowAnonymous / Authorize / RequirePermission from endpoint metadata or attributes
                var hasAllowAnonymous = endpointMetadata?.OfType<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>().Any() == true;
                var hasAuthorizeMeta = endpointMetadata?.Any(m => m.GetType().Name == "AuthorizeAttribute") == true;
                var hasRequirePermissionMeta = endpointMetadata?.Any(m => m.GetType().Name == "RequirePermissionAttribute") == true;

                var hasAllowAnonymousAttr = methodInfo?.GetCustomAttributes(true).OfType<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>().Any() == true
                                            || methodInfo?.DeclaringType?.GetCustomAttributes(true).OfType<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>().Any() == true;
                var hasAuthorizeAttr = methodInfo?.GetCustomAttributes(true).Any(a => a.GetType().Name == "AuthorizeAttribute") == true
                                    || methodInfo?.DeclaringType?.GetCustomAttributes(true).Any(a => a.GetType().Name == "AuthorizeAttribute") == true;
                var hasRequirePermissionAttr = methodInfo?.GetCustomAttributes(true).Any(a => a.GetType().Name == "RequirePermissionAttribute") == true
                                            || methodInfo?.DeclaringType?.GetCustomAttributes(true).Any(a => a.GetType().Name == "RequirePermissionAttribute") == true;

                var allowAnonymous = hasAllowAnonymous || hasAllowAnonymousAttr;
                var authorize = hasAuthorizeMeta || hasAuthorizeAttr;
                var requirePermission = hasRequirePermissionMeta || hasRequirePermissionAttr;

                if (docName == "public")
                {
                    // include actions explicitly anonymous OR those without any auth markers
                    return allowAnonymous || (!authorize && !requirePermission);
                }

                if (docName == "protected")
                {
                    // include actions that require auth/permissions
                    return authorize || requirePermission;
                }

                return false;
            });

            // Use full type names to avoid schema id collisions
            c.CustomSchemaIds(type => type.FullName);

            // JWT / Bearer definition (shared)
            c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Description = "Enter 'Bearer {token}'",
                Name = "Authorization",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            // OperationFilter will add per-operation security where needed
            c.OperationFilter<AuthorizeCheckOperationFilter>();

            // Optional: include XML comments if exist
            var xmlFiles = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "W2.Web.xml"),
                Path.Combine(AppContext.BaseDirectory, "W2.Application.xml"),
                Path.Combine(AppContext.BaseDirectory, "W2.Application.Contracts.xml")
            };

            foreach (var xmlFile in xmlFiles)
                if (File.Exists(xmlFile))
                    c.IncludeXmlComments(xmlFile, includeControllerXmlComments: true);
        });

        services.AddAbpSwaggerGen(options =>
        {
            options.SwaggerDoc("public", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "W2 API - Public", Version = "v1" });
            options.SwaggerDoc("protected", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "W2 API - Protected (requires token)", Version = "v1" });

            options.CustomSchemaIds(type => type.FullName);
        });
    }
    
    private void ConfigureElsa(ServiceConfigurationContext context, IConfiguration configuration)
    {
        var elsaConfigurationSection = configuration.GetSection(nameof(ElsaConfiguration));

        context.Services.AddElsa(options => options
            .UseEntityFrameworkPersistence(
                ef => ef.UsePostgreSql(elsaConfigurationSection.GetValue<string>(nameof(ElsaConfiguration.ConnectionString))), true)
            .AddConsoleActivities()
            .AddUserTaskActivities()
            .AddHttpActivities(elsaConfigurationSection.GetSection(nameof(ElsaConfiguration.Server)).Bind)
            .AddWorkflow<HelloWorkflow>()
            .AddEmailActivities(elsaConfigurationSection.GetSection(nameof(ElsaConfiguration.Smtp)).Bind)
            .AddQuartzTemporalActivities()
            .AddJavaScriptActivities()
            .AddActivitiesFrom<W2ApplicationModule>()
            .AddWorkflowsFrom<ElsaConfiguration>()
         );

        context.Services
            .AddElsaApiEndpoints()
            .AddRazorPages();

        context.Services
            .AddNotificationHandlersFrom<CustomSignalJavaScriptHandler>()
            .AddJavaScriptTypeDefinitionProvider<CustomTypeDefinitionProvider>();

        Configure<AbpAntiForgeryOptions>(options =>
        {
            // skip todo change
            options.AutoValidateIgnoredHttpMethods = new HashSet<string> { "PUT", "GET", "POST", "DELETE", "HEAD", "TRACE", "OPTIONS" };
            options.AutoValidateFilter = type => type.Assembly != typeof(Elsa.Server.Api.Endpoints.WorkflowRegistry.Get).Assembly;
        });
    }

    private void ConfigureExternalLogins(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services
            .AddAuthentication()
            .AddGoogle(options =>
            {
                options.ClientId = configuration["Authentication:Google:ClientId"];
                options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
                options.SaveTokens = true;
                options.CorrelationCookie.SameSite = SameSiteMode.Lax;
            });
        context.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });

        context.Services.ConfigureExternalCookie(options =>
        {
            options.Cookie.SameSite = SameSiteMode.Lax;
        });
    }

    private void ConfigureMultiTenancy(IConfiguration configuration)
    {
        Configure<AbpAspNetCoreMultiTenancyOptions>(options =>
        {
            options.TenantKey = W2Consts.TenantKey;
        });

        var domainFormat = configuration["TenantDomainFormat"];
        if (!domainFormat.IsNullOrEmpty())
        {
            Configure<AbpTenantResolveOptions>(options =>
            {
                options.AddDomainTenantResolver(domainFormat);
            });
        }
    }

    private void ConfigureRazorPages()
    {
        Configure<RazorPagesOptions>(options =>
        {
            options.Conventions.AuthorizeFolder("/WorkflowDefinitions", W2Permissions.WorkflowManagementWorkflowDefinitions);
            options.Conventions.AuthorizeFolder("/WorkflowInstances", W2Permissions.WorkflowManagementWorkflowInstances);
            options.Conventions.AllowAnonymousToFolder("/ViewDesigner");
        });
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        var workerManager = context.ServiceProvider.GetRequiredService<IBackgroundWorkerManager>();
        workerManager.AddAsync(context.ServiceProvider.GetRequiredService<SyncHrmWorker>());

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseAbpRequestLocalization();

        if (!env.IsDevelopment())
        {
            app.UseErrorPage();
            app.UseHttpsRedirection();
        }

        app.UseCorrelationId();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseJwtTokenMiddleware();
        app.UseHttpActivities();

        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }

        app.UseUnitOfWork();
        app.UseIdentityServer();
        app.UseCookiePolicy();
        app.UseAuthorization();

        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/public/swagger.json", "W2 API - Public");
            options.SwaggerEndpoint("/swagger/protected/swagger.json", "W2 API - Protected (requires token)");
            options.RoutePrefix = "swagger";
        });

        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
        app.UseForwardedHeaders();
    }
}
