using System.Threading.Tasks;

namespace W2.Data;

public interface IW2DbSchemaMigrator
{
    Task MigrateAsync();
}
