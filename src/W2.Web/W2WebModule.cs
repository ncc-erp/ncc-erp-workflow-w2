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
        context.Services.AddAuthentication()
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey
       (Encoding.UTF8.GetBytes(configuration["Jwt:Key"])),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                    AuthenticationType = "Identity.Application"
                };
            });
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
            c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
        });
        services.AddAbpSwaggerGen(
            options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "W2 API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);
                options.CustomSchemaIds(type => type.FullName);
            }
        );
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
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "W2 API");
        });
        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
        app.UseForwardedHeaders();
    }
}
