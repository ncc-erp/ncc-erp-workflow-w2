﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace W2.Tasks
{
    public interface ITaskAppService : IApplicationService
    {
        Task assignTask(string email, Guid userId, string workflowInstanceId, string Name, string ApproveSignal, string RejectSignal);
        Task<string> ApproveAsync(string id);
    }
}
