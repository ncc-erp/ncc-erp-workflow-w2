﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using W2.Scripting;
using W2.WorkflowDefinitions;
using W2.WorkflowInstances;

namespace W2.Tasks
{
    public interface ITaskAppService : IApplicationService
    {
        Task<string> assignTask(AssignTaskInput input, CancellationToken cancellationToken);
        Task<string> ApproveAsync(ApproveTasksInput input, CancellationToken cancellationToken);
        Task<PagedResultDto<W2TasksDto>> ListAsync(ListTaskstInput input);
        Task<string> RejectAsync(string id, string reason, CancellationToken cancellationToken);
        Task<string> ActionAsync(ListTaskActions input);
        // Task<string> CancelAsync(string id);
        Task<TaskDetailDto> GetDetailByIdAsync(string id);
        Task<PagedResultDto<W2TasksDto>> DynamicDataByIdAsync(TaskDynamicDataInput input);
        Task<Dictionary<string, string>> handleDynamicData(TaskDynamicDataInput input);
        Task<List<DynamicDataDto>> GetDynamicRawData(TaskDynamicDataInput input);
        Task<TaskDetailDto> GetTaskById(string id);
    }
}
