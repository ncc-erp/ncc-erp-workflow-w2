using Elsa.Activities.Http.Events;
using Elsa.Activities.Signaling.Models;
using Elsa.Activities.Signaling.Services;
using Elsa.Models;
using Elsa.Persistence;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Open.Linq.AsyncExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.SettingManagement;
using Volo.Abp.Settings;
using Volo.Abp.Users;
using W2.ExternalResources;
using W2.Permissions;
using W2.Scripting;
using W2.WorkflowDefinitions;
using W2.WorkflowInstances;
using static Volo.Abp.Identity.Settings.IdentitySettingNames;

namespace W2.Tasks
{
    [Authorize]
    public class TaskAppService : W2AppService, ITaskAppService
    {
        private readonly IRepository<W2Task, Guid> _taskRepository;
        private readonly ISignaler _signaler;
        private readonly IMediator _mediator;
        private readonly ICurrentUser _currentUser;

        public TaskAppService(IRepository<W2Task, Guid> taskRepository, ISignaler signaler, IMediator mediator, ICurrentUser currentUser)
        {
            _signaler = signaler;
            _taskRepository = taskRepository;
            _mediator = mediator;
            _currentUser = currentUser;
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

        public async Task createTask(string id)
        {
            //await _taskRepository.InsertAsync(CurrentTenant.Id, W2Settings.SocialLoginSettingsEnableSocialLogin, input.EnableSocialLogin.ToString());
        }


        public async Task<string> ApproveAsync([Required] string id)
        {
            var myTask = await _taskRepository.FirstOrDefaultAsync(x => x.Id == Guid.Parse(id));

            if (myTask == null || myTask.Status != W2TaskStatus.Pending || myTask.Email != _currentUser.Email)
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

        public async Task<string> RejectAsync([Required] string id, [Required] string reason)
        {
            var myTask = await _taskRepository.FirstOrDefaultAsync(x => x.Id == Guid.Parse(id));

            if (myTask == null  || myTask.Status != W2TaskStatus.Pending || myTask.Email != _currentUser.Email)
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
            myTask.Reason = reason;

            await _taskRepository.UpdateAsync(myTask);

            return "Reject successful";
        }

        public async Task<string> CancelAsync([Required] string id)
        {
            var myTask = await _taskRepository.FirstOrDefaultAsync(x => x.Id == Guid.Parse(id));

            if (myTask == null || myTask.Status != W2TaskStatus.Pending || myTask.Email != _currentUser.Email)
            {
                throw new UserFriendlyException(L["Exception:MyTaskNotValid"]);
            }

            myTask.Status = W2TaskStatus.Cancel;
            await _taskRepository.UpdateAsync(myTask);

            return "Cancel successful";
        }

        public async Task<PagedResultDto<W2TasksDto>> ListAsync(ListTaskstInput input)
        {
            var requestTasks = (await _taskRepository.GetListAsync()).Where(x =>
            {
                if (input.Status != null && Enum.IsDefined(typeof(W2TaskStatus), input.Status))
                {
                    return x.Status == input.Status && x.Email == _currentUser.Email;
                }
                return x.Email == _currentUser.Email;
            })
            .AsQueryable()
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

            var W2TaskList = new List<W2TasksDto>();
            foreach (var element in requestTasks)
            {
                var taskDto = new W2TasksDto
                {
                    WorkflowInstanceId = element.WorkflowInstanceId,
                    Email = element.Email,
                    Status = element.Status,
                    Name = element.Name,
                    Reason = element.Reason,
                    ApproveSignal = element.ApproveSignal,
                    RejectSignal = element.RejectSignal,
                };
                W2TaskList.Add(taskDto);
            }

            return new PagedResultDto<W2TasksDto>(requestTasks.Count(), W2TaskList);
        }
    }
}
