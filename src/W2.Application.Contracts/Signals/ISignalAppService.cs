using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace W2.Signals
{
    public interface ISignalAppService : IApplicationService
    {
        Task TriggerAsync(string token);
    }
}
