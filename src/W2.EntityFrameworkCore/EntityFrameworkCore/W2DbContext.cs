using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections.Generic;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.IdentityServer.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using W2.Tasks;
using W2.WorkflowDefinitions;
using W2.WorkflowInstances;

namespace W2.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class W2DbContext :
    AbpDbContext<W2DbContext>,
    IIdentityDbContext,
    ITenantManagementDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */

    #region Entities from the modules

    /* Notice: We only implemented IIdentityDbContext and ITenantManagementDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityDbContext and ITenantManagementDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */
    public DbSet<WorkflowInstanceStarter> WorkflowInstanceStarters { get; set; }
    public DbSet<WorkflowCustomInputDefinition> WorkflowCustomInputDefinitions { get; set; }

    //Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }

    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public W2DbContext(DbContextOptions<W2DbContext> options)
        : base(options)
    {

    }

    public DbSet<MyTask> MyTasks { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureIdentity();
        builder.ConfigureIdentityServer();
        builder.ConfigureFeatureManagement();
        builder.ConfigureTenantManagement();

        /* Configure your own tables/entities inside here */

        //builder.Entity<YourEntity>(b =>
        //{
        //    b.ToTable(W2Consts.DbTablePrefix + "YourEntities", W2Consts.DbSchema);
        //    b.ConfigureByConvention(); //auto configure for the base class props
        //    //...
        //});

        builder.Entity<WorkflowInstanceStarter>(b =>
        {
            b.ToTable("WorkflowInstanceStarters");
            b.Property(x => x.WorkflowInstanceId).IsRequired();
            b.Property(x => x.Input).HasConversion(new ElsaEFJsonValueConverter<Dictionary<string, string>>(), ValueComparer.CreateDefault(typeof(Dictionary<string, string>), false));
            b.HasIndex(x => x.WorkflowInstanceId);
        });

        builder.Entity<WorkflowCustomInputDefinition>(b =>
        {
            b.ToTable("WorkflowCustomInputDefinitions");
            b.Property(x => x.PropertyDefinitions).HasConversion(new ElsaEFJsonValueConverter<ICollection<WorkflowCustomInputPropertyDefinition>>(), ValueComparer.CreateDefault(typeof(ICollection<WorkflowCustomInputPropertyDefinition>), false));
            b.Property(x => x.WorkflowDefinitionId).IsRequired();
            b.HasIndex(x => x.WorkflowDefinitionId);
        });

        builder.Entity<MyTask>(b =>
        {
            b.ToTable("MyTasks");
        });
    }
}
