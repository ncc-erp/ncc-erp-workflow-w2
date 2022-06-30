using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace W2.WorkflowDefinitions
{
    public interface IWorkflowDefinitionAppService : IApplicationService
    {
        Task<WorkflowDefinitionSummaryDto> GetByDefinitionIdAsync(string definitionId);
        Task<ListResultDto<WorkflowDefinitionSummaryDto>> ListAllAsync();
        Task<string> CreateNewInstanceAsync(ExecuteWorkflowDefinitionDto input);
        Task CreateWorkflowInputDefinitionAsync(WorkflowCustomInputDefinitionDto input);
    }
}
