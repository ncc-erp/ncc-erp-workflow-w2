﻿using Elsa.Activities.Http.Events;
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
using Volo.Abp.Identity;
using W2.WorkflowInstances;
using Volo.Abp.Identity;
using W2.TaskActions;

namespace W2.Tasks
{
    [Authorize]
    public class TaskAppService : W2AppService, ITaskAppService
    {
        private readonly IRepository<W2Task, Guid> _taskRepository;
        private readonly IRepository<W2TaskActions, Guid> _taskActionsRepository;
        private readonly IIdentityUserRepository _userRepository;
        private readonly ISignaler _signaler;
        private readonly IMediator _mediator;
        private readonly ICurrentUser _currentUser;
        private readonly IWorkflowInstanceStore _workflowInstanceStore;
        private readonly IWorkflowDefinitionStore _workflowDefinitionStore;


        public TaskAppService(
            IRepository<W2Task, Guid> taskRepository,
            IRepository<W2TaskActions, Guid> taskActionsRepository,
            ISignaler signaler,
            IMediator mediator,
            ICurrentUser currentUser,
            IWorkflowInstanceStore workflowInstanceStore,
            IWorkflowDefinitionStore workflowDefinitionStore,
            IIdentityUserRepository userRepository)
        {
            _signaler = signaler;
            _taskRepository = taskRepository;
            _taskActionsRepository = taskActionsRepository;
            _mediator = mediator;
            _currentUser = currentUser;
            _workflowInstanceStore = workflowInstanceStore;
            _workflowDefinitionStore = workflowDefinitionStore;
            _userRepository = userRepository;
        }

        //[Authorize(W2Permissions.WorkflowManagementSettingsSocialLoginSettings)]
        public async Task assignTask(string email, Guid userId, string workflowInstanceId, string ApproveSignal, string RejectSignal, string Description, List<string> OtherActionSignals = default)
        {

            var workflowInstance = await _workflowInstanceStore.FindByIdAsync(workflowInstanceId);
            var workflowDefinitions = (await _workflowDefinitionStore.FindManyAsync(
                new ListAllWorkflowDefinitionsSpecification(CurrentTenantStrId, new string[] { workflowInstance.DefinitionId }))).FirstOrDefault();

            var actions = await _taskActionsRepository.InsertAsync(
                new W2TaskActions
                {
                    Author = userId,
                    OtherActionSignals = OtherActionSignals,
                    Status = W2TaskActionsStatus.Pending
                }
            );

            await _taskRepository.InsertAsync(new W2Task
            {
                TenantId = CurrentTenant.Id,
                Email = email,
                Author = userId,
                WorkflowInstanceId = workflowInstanceId,
                WorkflowDefinitionId = workflowInstance.DefinitionId,
                Status = W2TaskStatus.Pending,
                Name = workflowDefinitions.Name,
                Description = Description,
                ApproveSignal = ApproveSignal,
                RejectSignal = RejectSignal,
                OtherActionSignals = actions.Id.ToString(),
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

            var Inputs = new Dictionary<string, string>
            {
                { "Reason", $"{myTask.ApproveSignal}" },
                { "TriggeredBy", $"{_currentUser.Email}" }
            };

            var affectedWorkflows = await _signaler.TriggerSignalAsync(myTask.ApproveSignal, Inputs, myTask.WorkflowInstanceId).ToList();
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
                { "TriggeredBy", $"{_currentUser.Email}" }
            };

            var affectedWorkflows = await _signaler.TriggerSignalAsync(myTask.RejectSignal, Inputs, myTask.WorkflowInstanceId).ToList();

            var signal = new SignalModel(myTask.RejectSignal, myTask.WorkflowInstanceId);
            await _mediator.Publish(new HttpTriggeredSignal(signal, affectedWorkflows));

            myTask.Status = W2TaskStatus.Reject;
            myTask.Reason = reason;

            await _taskRepository.UpdateAsync(myTask);

            return "Reject successful";
        }

        public async Task<string> ActionAsync(ListTaskActions input)
        {
            var myTask = await _taskRepository.FirstOrDefaultAsync(x => x.Id == Guid.Parse(input.Id));

            if (myTask == null || myTask.Status != W2TaskStatus.Pending || myTask.Email != _currentUser.Email)
            {
                throw new UserFriendlyException(L["Exception:MyTaskNotValid"]);
            }

            var actions = await _taskActionsRepository.FirstOrDefaultAsync(x => x.Id == Guid.Parse(myTask.OtherActionSignals));

            if (actions == null || !actions.OtherActionSignals.Contains(input.Action) || actions.Status != W2TaskActionsStatus.Pending)
            {
                throw new UserFriendlyException(L["Exception:Action is not valid for this task."]);
            }

            var Inputs = new Dictionary<string, string>
            {
                { "Reason", $"{input.Action}" },
                { "TriggeredBy", $"{_currentUser.Email}" }
            };

            var affectedWorkflows = await _signaler.TriggerSignalAsync(input.Action, Inputs, myTask.WorkflowInstanceId).ToList();

            var signal = new SignalModel(input.Action, myTask.WorkflowInstanceId);
            await _mediator.Publish(new HttpTriggeredSignal(signal, affectedWorkflows));

            actions.Status = W2TaskActionsStatus.Approve;

            await _taskActionsRepository.UpdateAsync(actions);

            return "Send Action successful";
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
            var users = await _userRepository.GetListAsync();
            var tasks = await _taskRepository.GetListAsync();
            var actions = await _taskActionsRepository.GetListAsync();
            var hasWorkflowDefinitionId = !string.IsNullOrEmpty(input.WorkflowDefinitionId);
            var query = tasks.Join(users, x => x.Author, x => x.Id, (W2task, W2User) => new
            {
                W2User,
                W2task
            }).GroupJoin(actions, x => Guid.Parse(x.W2task.OtherActionSignals), x => x.Id, (joinedEntities, W2TaskActions) => new
            {
                W2TaskActions = W2TaskActions.FirstOrDefault(),
                joinedEntities.W2User,
                joinedEntities.W2task,
            });

            List<Func<W2Task, bool>> checks = new List<Func<W2Task, bool>>();
            var isAdmin = _currentUser.IsInRole("admin");
            if (!isAdmin)
            {
                query = query.Where(x => x.W2task.Email == _currentUser.Email);
            }
            if (input.KeySearch != null && !input.KeySearch.IsNullOrWhiteSpace() && isAdmin)
            {
                query = query.Where(x => x.W2task.Email.Contains(input.KeySearch));
            }

            if (input.Dates != null && !input.Dates.IsNullOrWhiteSpace())
            {
                query = query.Where(x => new DateTimeOffset(x.W2task.CreationTime).ToUnixTimeSeconds() >= DateTimeOffset.Parse(input.Dates).ToUnixTimeSeconds());
            }

            if (hasTaskStatus)
            {
                query = query.Where(x => x.W2task.Status == input.Status);
            }

            if (hasWorkflowDefinitionId)
            {
                query = query.Where(x => x.W2task.WorkflowDefinitionId == input.WorkflowDefinitionId);
            }

            var totalItemCount = query.Count();

                var requestTasks = query
                .OrderByDescending(task => task.W2task.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount).Select(x => new W2TasksDto
                {
                    Author = x.W2task.Author,
                    AuthorName = x.W2User.Name,
                    CreationTime = x.W2task.CreationTime,
                    Description = x.W2task.Description,
                    Email = x.W2task.Email,
                    Id = x.W2task.Id,
                    Name = x.W2task.Name,
                    OtherActionSignals = new TaskActionsDto
                    {
                        Id = x.W2TaskActions?.Id ?? Guid.Empty, // Use Guid.Empty as a default if Id is not available
                        OtherActionSignals = x.W2TaskActions?.OtherActionSignals,
                        Status = x.W2TaskActions.Status,
                    },
                    Reason = x.W2task.Reason,
                    Status = x.W2task.Status,
                    WorkflowDefinitionId = x.W2task.WorkflowDefinitionId,
                    WorkflowInstanceId = x.W2task.WorkflowInstanceId
                })
                .ToList();

            return new PagedResultDto<W2TasksDto>(totalItemCount, requestTasks);
        }

        public async Task<PagedResultDto<W2TasksStakeHoldersDto>> StakeHoldersAsync(ListTaskstInput input)
        {
            var query = (await _taskRepository.GetListAsync())
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .Select(x => x.Email).Distinct();
            var totalItemCount = query.Count();
            var stakeHolderEmails = query.ToList();
            var result = new List<W2TasksStakeHoldersDto>();
            foreach (var email in stakeHolderEmails)
            {
                var stakeHolder = new W2TasksStakeHoldersDto();
                stakeHolder.Name = (await _userRepository.FindByNormalizedEmailAsync(email.ToUpper())).Name;
                stakeHolder.Email = email;

                result.Add(stakeHolder);
            }

            return new PagedResultDto<W2TasksStakeHoldersDto>(totalItemCount, result);
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