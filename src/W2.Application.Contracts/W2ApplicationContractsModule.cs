using Volo.Abp.Account;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Refit;
using Volo.Abp.Modularity;
using Volo.Abp.ObjectExtending;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;
using W2.ExternalResources;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace W2;

[DependsOn(
    typeof(W2DomainSharedModule),
    typeof(AbpAccountApplicationContractsModule),
    typeof(AbpFeatureManagementApplicationContractsModule),
    typeof(AbpIdentityApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationContractsModule),
    typeof(AbpSettingManagementApplicationContractsModule),
    typeof(AbpTenantManagementApplicationContractsModule),
    typeof(AbpObjectExtendingModule)
)]
public class W2ApplicationContractsModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        W2DtoExtensions.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        context.Services
            .AddRefitClient<IProjectClientApi>(RefitExtensions.GetNewtonsoftJsonRefitSettings())
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["Apis:Project"]));
        context.Services
            .AddRefitClient<ITimesheetClientApi>(RefitExtensions.GetNewtonsoftJsonRefitSettings())
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["Apis:Timesheet"]));
        context.Services
            .AddRefitClient<IAntClientApi>(RefitExtensions.GetNewtonsoftJsonRefitSettings())
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["Apis:Ant"]));
        context.Services
            .AddRefitClient<IHrmClientApi>(RefitExtensions.GetNewtonsoftJsonRefitSettings())
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["Apis:Hrm"]))
            .ConfigureHttpClient(c => c.DefaultRequestHeaders.Add("x-secret-key", configuration["Apis:HrmCode"]));
    }
}
