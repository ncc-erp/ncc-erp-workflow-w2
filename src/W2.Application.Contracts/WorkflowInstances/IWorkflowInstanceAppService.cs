using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using W2.ExternalResources;

namespace W2.WorkflowInstances
{
    public interface IWorkflowInstanceAppService : IApplicationService
    {
        Task<string> CreateNewInstanceAsync(CreateNewWorkflowInstanceDto input);
        Task<PagedResultDto<WorkflowInstanceDto>> ListAsync(ListAllWorkflowInstanceInput input);
        Task CancelAsync(string id);
        Task DeleteAsync(string id);
        Task<WorkflowInstanceDto> GetByIdAsync(string id);
        Task<PagedResultDto<WFHDto>> GetWfhListAsync(ListAllWFHRequestInput input);
    }
}
