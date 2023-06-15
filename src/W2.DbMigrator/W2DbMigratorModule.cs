using W2.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Timing;
using System;
using W2.Configurations;
using Autofac.Core;
using Elsa.Persistence.EntityFramework.Core.Extensions;
using Elsa.Persistence.EntityFramework.PostgreSql;
using Microsoft.Extensions.Configuration;

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

        ConfigureElsa(context, configuration);
    }

    private void ConfigureElsa(ServiceConfigurationContext context, IConfiguration configuration)
    {
        var elsaConfigurationSection = configuration.GetSection(nameof(ElsaConfiguration));

        context.Services.AddElsa(options => options
            .UseEntityFrameworkPersistence(
                ef => ef.UsePostgreSql(elsaConfigurationSection.GetValue<string>(nameof(ElsaConfiguration.ConnectionString)))));
    }
}
