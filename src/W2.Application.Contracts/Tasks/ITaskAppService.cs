using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using W2.WorkflowDefinitions;
using W2.WorkflowInstances;

namespace W2.Tasks
{
    public interface ITaskAppService : IApplicationService
    {
        Task assignTask(string email, Guid userId, string workflowInstanceId, string ApproveSignal, string RejectSignal, string OtherActionSignal, string Description);
        Task<string> ApproveAsync(string id);
        Task<PagedResultDto<W2TasksDto>> ListAsync(ListTaskstInput input);
        Task<string> RejectAsync(string id, string reason);
        Task<string> CancelAsync(string id);
        Task<TaskDetailDto> GetDetailByIdAsync(string id);
    }
}
