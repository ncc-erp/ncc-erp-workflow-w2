using W2.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Timing;
using System;

namespace W2.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(W2EntityFrameworkCoreModule),
    typeof(W2ApplicationContractsModule)
    )]
public class W2DbMigratorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpBackgroundJobOptions>(options => options.IsJobExecutionEnabled = false);

        var configuration = context.Services.GetConfiguration();
        Configure<Configurations.TenantConfiguration>(configuration.GetSection(nameof(Configurations.TenantConfiguration)));

        Configure<AbpClockOptions>(options =>
        {
            options.Kind = DateTimeKind.Utc;
        });
    }
}
