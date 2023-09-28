﻿using Elsa.Activities.Http.Events;
using Elsa.Activities.Signaling.Models;
using Elsa.Activities.Signaling.Services;
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
using Elsa.Persistence;
using W2.Specifications;
using Elsa;
using Newtonsoft.Json;
using Volo.Abp.Identity;
using W2.TaskEmail;
using Humanizer;
using static IdentityServer4.Models.IdentityResources;
using W2.TaskActions;
using static Volo.Abp.Identity.IdentityPermissions;
using Volo.Abp.ObjectMapping;

namespace W2.Tasks
{
    [Authorize]
    public class TaskAppService : W2AppService, ITaskAppService, ITaskEmailService
    {
        private readonly IRepository<W2Task, Guid> _taskRepository;
        private readonly IRepository<W2TaskEmail, Guid> _taskEmailRepository;
        private readonly IIdentityUserRepository _userRepository;
        private readonly ISignaler _signaler;
        private readonly IMediator _mediator;
        private readonly ICurrentUser _currentUser;
        private readonly IWorkflowInstanceStore _workflowInstanceStore;
        private readonly IWorkflowDefinitionStore _workflowDefinitionStore;


        public TaskAppService(
            IRepository<W2Task, Guid> taskRepository,
            IRepository<W2TaskEmail, Guid> taskEmailRepository,
            ISignaler signaler,
            IMediator mediator,
            ICurrentUser currentUser,
            IWorkflowInstanceStore workflowInstanceStore,
            IWorkflowDefinitionStore workflowDefinitionStore,
            IIdentityUserRepository userRepository)
        {
            _signaler = signaler;
            _taskRepository = taskRepository;
            _taskEmailRepository = taskEmailRepository;
            _mediator = mediator;
            _currentUser = currentUser;
            _workflowInstanceStore = workflowInstanceStore;
            _workflowDefinitionStore = workflowDefinitionStore;
            _userRepository = userRepository;
        }

        public async Task assignTask(AssignTaskInput input)
        {
            var workflowInstance = await _workflowInstanceStore.FindByIdAsync(input.WorkflowInstanceId);
            var workflowDefinitions = (await _workflowDefinitionStore.FindManyAsync(
                new ListAllWorkflowDefinitionsSpecification(CurrentTenantStrId, new string[] { workflowInstance.DefinitionId }))).FirstOrDefault();

            var task = await _taskRepository.InsertAsync(new W2Task
            {
                TenantId = CurrentTenant.Id,
                Author = input.UserId,
                WorkflowInstanceId = input.WorkflowInstanceId,
                WorkflowDefinitionId = workflowInstance.DefinitionId,
                Status = W2TaskStatus.Pending,
                Name = workflowDefinitions.Name,
                Description = input.Description,
                ApproveSignal = input.ApproveSignal,
                RejectSignal = input.RejectSignal,
                OtherActionSignals = input.OtherActionSignals
            });

            foreach (string email in input.EmailTo)
            {
                await _taskEmailRepository.InsertAsync(new W2TaskEmail
                {
                    Email = email,
                    TaskId = task.Id.ToString(),
                });
            }
        }

        public async Task createTask(string id) { }


        public async Task<string> ApproveAsync([Required] string id)
        {
            var myTask = await _taskRepository.FirstOrDefaultAsync(x => x.Id == Guid.Parse(id));
            if (myTask == null || myTask.Status != W2TaskStatus.Pending)
            {
                throw new UserFriendlyException(L["Exception:MyTaskNotValid"]);
            }

            var taskEmail = (await _taskEmailRepository.GetListAsync(x => x.TaskId == myTask.Id.ToString()))
                    .Where(x => x.Email == _currentUser.Email && x.TaskId == myTask.Id.ToString())
                    .ToList().FirstOrDefault();

            var isAdmin = _currentUser.IsInRole("admin");
            if (!isAdmin && taskEmail == null)
            {
                throw new UserFriendlyException(L["Exception:No Permission"]);
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
            myTask.UpdatedBy = _currentUser.Email;
            await _taskRepository.UpdateAsync(myTask);

            return "Approval successful";
        }

        public async Task<string> RejectAsync([Required] string id, [Required] string reason)
        {
            var myTask = await _taskRepository.FirstOrDefaultAsync(x => x.Id == Guid.Parse(id));
            if (myTask == null || myTask.Status != W2TaskStatus.Pending)
            {
                throw new UserFriendlyException(L["Exception:MyTaskNotValid"]);
            }

            var taskEmail = (await _taskEmailRepository.GetListAsync(x => x.TaskId == myTask.Id.ToString()))
                    .Where(x => x.Email == _currentUser.Email && x.TaskId == myTask.Id.ToString())
                    .ToList().FirstOrDefault();

            var isAdmin = _currentUser.IsInRole("admin");
            if (!isAdmin && taskEmail == null)
            {
                throw new UserFriendlyException(L["Exception:No Permission"]);
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
            myTask.UpdatedBy = _currentUser.Email;
            myTask.Reason = reason;

            await _taskRepository.UpdateAsync(myTask);

            return "Reject successful";
        }

        public async Task<string> ActionAsync(ListTaskActions input)
        {
            var myTask = await _taskRepository.FirstOrDefaultAsync(x => x.Id == Guid.Parse(input.Id));
            if (myTask == null || myTask.Status != W2TaskStatus.Pending)
            {
                throw new UserFriendlyException(L["Exception:MyTaskNotValid"]);
            }

            var taskEmail = (await _taskEmailRepository.GetListAsync(x => x.TaskId == myTask.Id.ToString()))
                    .Where(x => x.Email == _currentUser.Email && x.TaskId == myTask.Id.ToString())
                    .ToList().FirstOrDefault();

            var isAdmin = _currentUser.IsInRole("admin");
            if (!isAdmin && taskEmail == null)
            {
                throw new UserFriendlyException(L["Exception:No Permission"]);
            }

            if (!myTask.OtherActionSignals.Contains(input.Action))
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

            return "Send Action successful";
        }

        public async Task<PagedResultDto<W2TasksDto>> ListAsync(ListTaskstInput input)
        {
            var hasTaskStatus = input.Status != null && Enum.IsDefined(typeof(W2TaskStatus), input.Status);
            var users = await _userRepository.GetListAsync();
            var tasks = await _taskRepository.GetListAsync();
            var taskEmail = await _taskEmailRepository.GetListAsync();
            var hasWorkflowDefinitionId = !string.IsNullOrEmpty(input.WorkflowDefinitionId);

            var query = from task in tasks
                        join user in users on task.Author equals user.Id
                        join email in taskEmail on new { TaskID = task.Id.ToString() } equals new { TaskID = email.TaskId }
                        let emailList = (
                            from email in taskEmail
                            where email.TaskId == task.Id.ToString()
                            select email.Email
                        ).ToList()
                        let actionList = (
                            from action in actionGroup.DefaultIfEmpty()
                            select action != null ? new TaskActionsDto
                            {
                                OtherActionSignal = action.OtherActionSignal,
                                Status = action.Status
                            } : null
                        ).OrderBy(action => action?.OtherActionSignal).ToList()
                        select new
                        {
                            W2TaskEmail = email,
                            W2User = user,
                            W2task = task,
                            EmailTo = emailList
                        };


            List<Func<W2Task, bool>> checks = new List<Func<W2Task, bool>>();
            var isAdmin = _currentUser.IsInRole("admin");
            if (!isAdmin)
            {
                query = query.Where(x => x.W2TaskEmail.Email == _currentUser.Email);
            }
            if (!string.IsNullOrWhiteSpace(input.KeySearch) && isAdmin)
            {
                string keySearch = input.KeySearch.Trim();
                query = query.Where(x => x.W2TaskEmail.Email.Contains(keySearch));
            }

            if (!input.Dates.IsNullOrWhiteSpace())
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

            var totalItemCount = query
                .GroupBy(x => x.W2task.Id)
                .Select(group => group.FirstOrDefault())
                .Count();

            var requestTasks = query
                .GroupBy(x => x.W2task.Id) 
                .Select(group => group.FirstOrDefault())
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
                    EmailTo = x.EmailTo,
                    OtherActionSignals = x.W2task.OtherActionSignals,
                    Reason = x.W2task.Reason,
                    Status = x.W2task.Status,
                    WorkflowDefinitionId = x.W2task.WorkflowDefinitionId,
                    WorkflowInstanceId = x.W2task.WorkflowInstanceId
                })
                .ToList();

            return new PagedResultDto<W2TasksDto>(totalItemCount, requestTasks);
        }

        public async Task<TaskDetailDto> GetDetailByIdAsync(string id)
        {
            var myTask = await _taskRepository.FirstOrDefaultAsync(x => x.Id == Guid.Parse(id));
            var taskAction = await _taskActionsRepository.GetListAsync(x => x.TaskId == myTask.Id.ToString());
            var query = from task in new List<W2Task> { myTask }
                       join action in taskAction on task.Id.ToString() equals action.TaskId into actionGroup
                       let actionList = (
                            from action in actionGroup.DefaultIfEmpty()
                            select action != null ? new TaskActionsDto
                            {
                                OtherActionSignal = action.OtherActionSignal,
                                Status = action.Status
                            } : null
                        ).OrderBy(action => action?.OtherActionSignal).ToList()
                        select new
                        {
                            W2task = task,
                            OtherActionSignals = actionList.All(a => a != null) ? actionList : null
                        };


            var workflowInstanceId = myTask.WorkflowInstanceId;
            var workflowInstance = await _workflowInstanceStore.FindByIdAsync(workflowInstanceId);

            var data = workflowInstance.Variables.Data;

            var taskDto = ObjectMapper.Map<W2Task, W2TasksDto>(query.FirstOrDefault()?.W2task);
            var taskDetailDto = new TaskDetailDto
            {
                Tasks = taskDto,
                OtherActionSignals = query.FirstOrDefault()?.OtherActionSignals,
                Input = data,
            };

            return taskDetailDto;
        }
    }
}