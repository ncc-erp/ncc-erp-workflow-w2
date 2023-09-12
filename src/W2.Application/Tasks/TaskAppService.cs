using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.SettingManagement;
using Volo.Abp.Settings;
using W2.Permissions;
using W2.WorkflowDefinitions;

namespace W2.Tasks
{
    [Authorize]
    public class TaskAppService : W2AppService, ITaskAppService
    {
        private readonly IRepository<W2Task, Guid> _taskRepository;

        public TaskAppService(IRepository<W2Task, Guid> taskRepository)
        {
            _taskRepository = taskRepository;
        }

        //[Authorize(W2Permissions.WorkflowManagementSettingsSocialLoginSettings)]
        public async Task assignTask(string email, Guid userId, string workflowInstanceId)
        {
            await _taskRepository.InsertAsync(new W2Task
            {
                TenantId = CurrentTenant.Id,
                Email = email,
                Author = userId,
                WorkflowInstanceId = workflowInstanceId,
                Status = W2TaskStatus.Pending
            });
        }

        public async Task createTask(string email)
        {
            //await _taskRepository.InsertAsync(CurrentTenant.Id, W2Settings.SocialLoginSettingsEnableSocialLogin, input.EnableSocialLogin.ToString());
        }
    }
}
