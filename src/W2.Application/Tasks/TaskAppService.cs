using Elsa.Activities.Http.Events;
using Elsa.Activities.Signaling.Models;
using Elsa.Activities.Signaling.Services;
using Elsa.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Open.Linq.AsyncExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using Elsa.Persistence.Specifications;
using Elsa.Persistence.Specifications.WorkflowInstances;
using Elsa.Persistence;
using W2.Specifications;
using Elsa;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using W2.WorkflowInstances;

namespace W2.Tasks
{
    [Authorize]
    public class TaskAppService : W2AppService, ITaskAppService
    {
        private readonly IRepository<W2Task, Guid> _taskRepository;
        private readonly ISignaler _signaler;
        private readonly IMediator _mediator;
        private readonly ICurrentUser _currentUser;
        private readonly IWorkflowInstanceStore _workflowInstanceStore;
        private readonly IWorkflowDefinitionStore _workflowDefinitionStore;

        private readonly ILogger<TaskAppService> _logger;


        public TaskAppService(IRepository<W2Task, Guid> taskRepository,
            ISignaler signaler,
            IMediator mediator,
            ICurrentUser currentUser,
            IWorkflowInstanceStore workflowInstanceStore,
            IWorkflowDefinitionStore workflowDefinitionStore,
            ILogger<TaskAppService> logger)
        {
            _signaler = signaler;
            _taskRepository = taskRepository;
            _mediator = mediator;
            _currentUser = currentUser;
            _workflowInstanceStore = workflowInstanceStore;
            _workflowDefinitionStore = workflowDefinitionStore;
            _logger = logger;
        }

        //[Authorize(W2Permissions.WorkflowManagementSettingsSocialLoginSettings)]
        public async Task assignTask(string email, Guid userId, string workflowInstanceId, string ApproveSignal, string RejectSignal)
        {

            var workflowInstance = await _workflowInstanceStore.FindByIdAsync(workflowInstanceId);
            var workflowDefinitions = (await _workflowDefinitionStore.FindManyAsync(
                new ListAllWorkflowDefinitionsSpecification(CurrentTenantStrId, new string[] { workflowInstance.DefinitionId }))).FirstOrDefault();

            await _taskRepository.InsertAsync(new W2Task
            {
                TenantId = CurrentTenant.Id,
                Email = email,
                Author = userId,
                WorkflowInstanceId = workflowInstanceId,
                WorkflowDefinitionId = workflowInstance.DefinitionId,
                Status = W2TaskStatus.Pending,
                Name = workflowDefinitions.Name,
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

            if (myTask == null || myTask.Status != W2TaskStatus.Pending || myTask.Email != _currentUser.Email)
            {
                throw new UserFriendlyException(L["Exception:MyTaskNotValid"]);
            }

            var Inputs = new Dictionary<string, string>
            {
                { "Reason", $"{reason}" },
                { "TriggeredBy", $"{myTask.Email}" }
            };

            var affectedWorkflows = await _signaler.TriggerSignalAsync(myTask.RejectSignal, Inputs, myTask.WorkflowInstanceId).ToList();

            var signal = new SignalModel(myTask.RejectSignal, myTask.WorkflowInstanceId);
            await _mediator.Publish(new HttpTriggeredSignal(signal, affectedWorkflows));

            myTask.Status = W2TaskStatus.Reject;
            myTask.Reason = reason;

            await _taskRepository.UpdateAsync(myTask);

            return "Reject successful";
        }

        // public async Task<string> CancelAsync([Required] string id)
        // {
        //     var myTask = await _taskRepository.FirstOrDefaultAsync(x => x.Id == Guid.Parse(id));

        //     if (myTask == null || myTask.Status != W2TaskStatus.Pending || myTask.Email != _currentUser.Email)
        //     {
        //         throw new UserFriendlyException(L["Exception:MyTaskNotValid"]);
        //     }

        //     myTask.Status = W2TaskStatus.Cancel;
        //     await _taskRepository.UpdateAsync(myTask);

        //     return "Cancel successful";
        // }

        public async Task<PagedResultDto<W2TasksDto>> ListAsync(ListTaskstInput input)
        {
            var hasTaskStatus = input.Status != null && Enum.IsDefined(typeof(W2TaskStatus), input.Status);
            var hasWorkflowDefinitionId = !string.IsNullOrEmpty(input.WorkflowDefinitionId);
            var query = (await _taskRepository.GetListAsync()).AsQueryable();
            List<Func<W2Task, bool>> checks = new List<Func<W2Task, bool>>();
            var isAdmin = _currentUser.IsInRole("admin");
            if (!isAdmin)
            {
                query = query.Where(x => x.Email == _currentUser.Email);
            }
            if (!input.Email.IsNullOrWhiteSpace() && isAdmin)
            {
                query = query.Where(x => x.Email == input.Email);
            }

            if (!input.Dates.IsNullOrWhiteSpace())
            {
                query = query.Where(x => new DateTimeOffset(x.CreationTime).ToUnixTimeSeconds() >= DateTimeOffset.Parse(input.Dates).ToUnixTimeSeconds());
            }

            if (hasTaskStatus)
            {
                query = query.Where(x => x.Status == input.Status);
            }

            if (hasWorkflowDefinitionId)
            {
                query = query.Where(x => x.WorkflowDefinitionId == input.WorkflowDefinitionId);
            }

            var totalItemCount = query.Count();

            var requestTasks = query
                .OrderByDescending(task => task.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToList();

            var W2TaskList = ObjectMapper.Map<List<W2Task>, List<W2TasksDto>>(requestTasks);

            return new PagedResultDto<W2TasksDto>(totalItemCount, W2TaskList);
        }

        public async Task<TaskDetailDto> GetDetailByIdAsync(string id)
        {
            var myTask = await _taskRepository.FirstOrDefaultAsync(x => x.Id == Guid.Parse(id));
            var workflowInstanceId = myTask.WorkflowInstanceId;
            var workflowInstance = await _workflowInstanceStore.FindByIdAsync(workflowInstanceId);

            var data = workflowInstance.Variables.Data;
            var taskDto = ObjectMapper.Map<W2Task, W2TasksDto>(myTask);
            var taskDetailDto = new TaskDetailDto
            {
                Tasks = taskDto,
                Input = data,
            };

            return taskDetailDto;
        }
    }
}
