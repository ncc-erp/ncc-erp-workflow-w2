﻿using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Account;
using Volo.Abp.AutoMapper;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;
using W2.HostedService;
using W2.Jobs;
using W2.Komu;
using W2.Mezon;

namespace W2;

[DependsOn(
    typeof(W2DomainModule),
    typeof(AbpAccountApplicationModule),
    typeof(W2ApplicationContractsModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule)
    )]
public class W2ApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<ITaskQueue, TaskQueue>();
        context.Services.AddSingleton<IKomuAppService, KomuAppService>();
        context.Services.AddHostedService<EmailHostedService>();
        context.Services.AddSingleton<IMezonAppService, MezonAppService>();

        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<W2ApplicationModule>();
        });

        var configuration = context.Services.GetConfiguration();
        Configure<Configurations.KomuConfiguration>(configuration.GetSection(nameof(Configurations.KomuConfiguration)));
        Configure<Configurations.ApiConfiguration>(configuration.GetSection(nameof(Configurations.ApiConfiguration)));
    }
}
