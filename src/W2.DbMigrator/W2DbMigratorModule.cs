using W2.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Modularity;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

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
    }
}
