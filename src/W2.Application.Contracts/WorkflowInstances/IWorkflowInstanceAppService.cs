﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using W2.ExternalResources;

namespace W2.WorkflowInstances
{
    public interface IWorkflowInstanceAppService : IApplicationService
    {
        Task<object> CreateNewInstanceAsync(CreateNewWorkflowInstanceDto input);
        Task<PagedResultDto<WorkflowInstanceDto>> ListAsync(ListAllWorkflowInstanceInput input);
        Task<string> CancelAsync(string id);
        Task<WorkflowInstanceDto> GetByIdAsync(string id);
        Task<PagedResultDto<WFHRequestDto>> GetWfhListAsync(ListAllWFHRequestInput input);
        Task<WorkflowInstanceDetailDto> GetDetailByIdAsync(string id);
    }
}
