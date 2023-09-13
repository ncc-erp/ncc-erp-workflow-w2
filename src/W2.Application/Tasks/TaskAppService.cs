using Elsa.Activities.Http.Events;
using Elsa.Activities.Signaling.Models;
using Elsa.Activities.Signaling.Services;
using Elsa.Models;
using Elsa.Persistence;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Open.Linq.AsyncExtensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.SettingManagement;
using Volo.Abp.Settings;
using W2.Permissions;
using W2.WorkflowDefinitions;
using static Volo.Abp.Identity.Settings.IdentitySettingNames;

namespace W2.Tasks
{
    [Authorize]
    public class TaskAppService : W2AppService, ITaskAppService
    {
        private readonly IRepository<W2Task, Guid> _taskRepository;
        private readonly ISignaler _signaler;
        private readonly IMediator _mediator;

        public TaskAppService(IRepository<W2Task, Guid> taskRepository, ISignaler signaler, IMediator mediator)
        {
            _signaler = signaler;
            _taskRepository = taskRepository;
            _mediator = mediator;
        }

        //[Authorize(W2Permissions.WorkflowManagementSettingsSocialLoginSettings)]
        public async Task assignTask(string email, Guid userId, string workflowInstanceId, string Name, string ApproveSignal, string RejectSignal)
        {
            await _taskRepository.InsertAsync(new W2Task
            {
                TenantId = CurrentTenant.Id,
                Email = email,
                Author = userId,
                WorkflowInstanceId = workflowInstanceId,
                Status = W2TaskStatus.Pending,
                Name = Name,
                ApproveSignal = ApproveSignal, 
                RejectSignal = RejectSignal 
            });
        }

        public async Task createTask(string email)
        {
            //await _taskRepository.InsertAsync(CurrentTenant.Id, W2Settings.SocialLoginSettingsEnableSocialLogin, input.EnableSocialLogin.ToString());
        }


        public async Task<string> ApproveAsync(string id)
        {
            var myTask = await _taskRepository.FirstOrDefaultAsync(x => x.Id == Guid.Parse(id));

            if (myTask == null || myTask.Status != W2TaskStatus.Pending)
            {
                throw new UserFriendlyException(L["Exception:MyTaskNotValid"]);
            }

            var affectedWorkflows = await _signaler.TriggerSignalAsync(myTask.ApproveSignal, null, myTask.WorkflowInstanceId).ToList();
            var signal = new SignalModel(myTask.ApproveSignal, myTask.WorkflowInstanceId);
            await _mediator.Publish(new HttpTriggeredSignal(signal, affectedWorkflows));

            myTask.Status = W2TaskStatus.Approve;
            await _taskRepository.UpdateAsync(myTask);

            return "Approval successful";
        }

        public async Task<string> RejectAsync(string id)
        {
            var myTask = await _taskRepository.FirstOrDefaultAsync(x => x.Id == Guid.Parse(id));

            if (myTask == null || myTask.Status != W2TaskStatus.Pending)
            {
                throw new UserFriendlyException(L["Exception:MyTaskNotValid"]);
            }

            var Inputs = new Dictionary<string, string>
            {
                { "TriggeredBy", $"({myTask.Email})" }
            };

            var affectedWorkflows = await _signaler.TriggerSignalAsync(myTask.RejectSignal, Inputs, myTask.WorkflowInstanceId).ToList();

            var signal = new SignalModel(myTask.RejectSignal, myTask.WorkflowInstanceId);
            await _mediator.Publish(new HttpTriggeredSignal(signal, affectedWorkflows));

            myTask.Status = W2TaskStatus.Reject;
            await _taskRepository.UpdateAsync(myTask);

            return "Reject successful";
        }

        public async Task<string> CancelAsync(string id)
        {
            var myTask = await _taskRepository.FirstOrDefaultAsync(x => x.Id == Guid.Parse(id));

            if (myTask == null || myTask.Status != W2TaskStatus.Pending)
            {
                throw new UserFriendlyException(L["Exception:MyTaskNotValid"]);
            }

            myTask.Status = W2TaskStatus.Cancel;
            await _taskRepository.UpdateAsync(myTask);

            return "Cancel successful";
        }
    }
}
