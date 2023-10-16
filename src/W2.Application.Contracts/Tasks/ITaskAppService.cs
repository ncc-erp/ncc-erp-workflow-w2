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
        Task<string> assignTask(AssignTaskInput input);
        Task<string> ApproveAsync(ApproveTasksInput input);
        Task<PagedResultDto<W2TasksDto>> ListAsync(ListTaskstInput input);
        Task<string> RejectAsync(string id, string reason);
        Task<string> ActionAsync(ListTaskActions input);
        // Task<string> CancelAsync(string id);
        Task<TaskDetailDto> GetDetailByIdAsync(string id);
        Task<PagedResultDto<W2TasksDto>> DynamicDataByIdAsync(TaskDynamicDataInput input);
        Task<object> getAllDynamicData(TaskDynamicDataInput input);
    }
}
