using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace W2.Data;

/* This is used if database provider does't define
 * IW2DbSchemaMigrator implementation.
 */
public class NullW2DbSchemaMigrator : IW2DbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
