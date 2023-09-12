using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace W2.WorkflowInstances
{
    public interface IWorkflowInstanceAppService : IApplicationService
    {
        Task<string> CreateNewInstanceAsync(CreateNewWorkflowInstanceDto input);
        Task<PagedResultDto<WorkflowInstanceDto>> ListAsync(ListAllWorkflowInstanceInput input);
        Task<string> ApproveAsync(string id);
        Task PendingAsync(string id);
        Task CancelAsync(string id);
        Task DeleteAsync(string id);
        Task<WorkflowInstanceDto> GetByIdAsync(string id);
    }
}
