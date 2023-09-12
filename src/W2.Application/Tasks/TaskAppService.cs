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
        private readonly IRepository<MyTask, Guid> _taskRepository;

        public TaskAppService(IRepository<MyTask, Guid> taskRepository)
        {
            _taskRepository = taskRepository;
        }

        //[Authorize(W2Permissions.WorkflowManagementSettingsSocialLoginSettings)]
        public async Task assignTask(string email)
        {
            //await _taskRepository.InsertAsync(CurrentTenant.Id, W2Settings.SocialLoginSettingsEnableSocialLogin, input.EnableSocialLogin.ToString());
        }

        public async Task createTask(string email)
        {
            //await _taskRepository.InsertAsync(CurrentTenant.Id, W2Settings.SocialLoginSettingsEnableSocialLogin, input.EnableSocialLogin.ToString());
        }

    }
}
