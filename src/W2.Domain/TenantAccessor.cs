using Elsa.Services;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;

namespace W2
{
    public class TenantAccessor : ITenantAccessor, IScopedDependency
    {
        private readonly ICurrentTenant _currentTenant;

        public TenantAccessor(ICurrentTenant currentTenant)
        {
            _currentTenant = currentTenant;
        }

        public Task<string> GetTenantIdAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_currentTenant?.Id?.ToString());
        }
    }
}
