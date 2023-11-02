using Elsa.Activities.Http.Events;
using Elsa.Activities.Signaling.Models;
using Elsa.Activities.Signaling.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Open.Linq.AsyncExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using W2.Authentication;
using W2.TaskActions;
using W2.TaskEmail;
using W2.Tasks;
using W2.WorkflowInstances;

namespace W2.Public
{
    public class PublicAppService : W2AppService
    {
        private readonly IRepository<W2Task, Guid> _taskRepository;
        private readonly IRepository<W2TaskEmail, Guid> _taskEmailRepository;
        private readonly IIdentityUserRepository _userRepository;
        private readonly IRepository<W2TaskActions, Guid> _taskActionsRepository;
        private readonly ISignaler _signaler;
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;

        public PublicAppService(
            IRepository<W2Task, Guid> taskRepository,
            IRepository<W2TaskEmail, Guid> taskEmailRepository,
            IIdentityUserRepository userRepository,
            IRepository<W2TaskActions, Guid> taskActionsRepository,
            ISignaler signaler,
            IMediator mediator,
            IConfiguration configuration
        ) {
            _taskRepository = taskRepository;
            _taskEmailRepository = taskEmailRepository;
            _userRepository = userRepository;
            _taskActionsRepository = taskActionsRepository;
            _signaler = signaler;
            _mediator = mediator;
            _configuration = configuration;
        }

        private async Task<bool> isValidMail(string email)
        {
            var mailDomain = "@" + _configuration["Authentication:Google:Domain"];  

            if (string.IsNullOrWhiteSpace(email) || !email.Contains(mailDomain))
            {
                throw new UserFriendlyException("Invalid Email!");
            }

            return true;
        }

        [HttpPost]
        [ExternalAuthentication]
        public async Task<PagedResultDto<W2TasksDto>> getListTasksByEmail(ListTasksInputExternal input)
        {
            await isValidMail(input.Email);

            var users = await _userRepository.GetListAsync();
            var tasks = await _taskRepository.GetListAsync();
            var taskEmail = await _taskEmailRepository.GetListAsync();
            var taskAction = await _taskActionsRepository.GetListAsync();

            var query = from task in tasks
                        join user in users on task.Author equals user.Id
                        join email in taskEmail on new { TaskID = task.Id.ToString() } equals new { TaskID = email.TaskId }
                        join action in taskAction on new { TaskID = task.Id.ToString() } equals new { TaskID = action.TaskId } into actionGroup
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
                            EmailTo = emailList,
                            OtherActionSignals = actionList.All(a => a != null) ? actionList : null
                        };

            query = query.Where(x => x.W2TaskEmail.Email.Contains(input.Email.Trim()));

            if (input.Status != null)
            {
                query = query.Where(x => input.Status.Contains(x.W2task.Status));
            }

            if (input.RequestName != null)
            {
                query = query.Where(x => input.RequestName.Contains(x.W2task.Name));
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
                    Email = x.W2User.Email,
                    Id = x.W2task.Id,
                    Name = x.W2task.Name,
                    EmailTo = x.EmailTo,
                    DynamicActionData = x.W2task.DynamicActionData,
                    OtherActionSignals = x.OtherActionSignals,
                    Reason = x.W2task.Reason,
                    Status = x.W2task.Status,
                    WorkflowDefinitionId = x.W2task.WorkflowDefinitionId,
                    WorkflowInstanceId = x.W2task.WorkflowInstanceId
                })
                .ToList();

            return new PagedResultDto<W2TasksDto>(totalItemCount, requestTasks);
        }

        [HttpPost]
        [ExternalAuthentication]
        public async Task<Dictionary<string, string>> ApproveTask(ApproveTasksInput input)
        {
            await isValidMail(input.Email);

            var userEmail = input.Email.Trim();

            var myTask = await _taskRepository.FirstOrDefaultAsync(x => x.Id == Guid.Parse(input.Id));
            if (myTask == null || myTask.Status != W2TaskStatus.Pending)
            {
                throw new UserFriendlyException(L["Exception:MyTaskNotValid"]);
            }

            var taskEmail = (await _taskEmailRepository.GetListAsync(x => x.TaskId == myTask.Id.ToString()))
                    .Where(x => x.Email == userEmail && x.TaskId == myTask.Id.ToString())
                    .ToList().FirstOrDefault();

            if (taskEmail == null)
            {
                throw new UserFriendlyException(L["Exception:No Permission"]);
            }

            var Inputs = new Dictionary<string, string>
            {
                { "Reason", $"{myTask.ApproveSignal}" },
                { "TriggeredBy", $"{userEmail}" }
            };

            if (!string.IsNullOrEmpty(input.DynamicActionData))
            {
                myTask.DynamicActionData = input.DynamicActionData;
            }

            var affectedWorkflows = await _signaler.TriggerSignalAsync(myTask.ApproveSignal, Inputs, myTask.WorkflowInstanceId).ToList();
            var signal = new SignalModel(myTask.ApproveSignal, myTask.WorkflowInstanceId);
            await _mediator.Publish(new HttpTriggeredSignal(signal, affectedWorkflows));

            myTask.Status = W2TaskStatus.Approve;
            myTask.UpdatedBy = userEmail;
            await _taskRepository.UpdateAsync(myTask);

            return new Dictionary<string, string> {
                { "id", input.Id.ToString() },
                { "message", "Approve Request Successfully!" }
            };
        }

        [HttpPost]
        [ExternalAuthentication]
        public async Task<Dictionary<string, string>> RejectTask(RejectTaskInput input)
        {
            await isValidMail(input.Email);

            var myTask = await _taskRepository.FirstOrDefaultAsync(x => x.Id == Guid.Parse(input.Id));
            if (myTask == null || myTask.Status != W2TaskStatus.Pending)
            {
                throw new UserFriendlyException(L["Exception:MyTaskNotValid"]);
            }

            var taskEmail = (await _taskEmailRepository.GetListAsync(x => x.TaskId == myTask.Id.ToString()))
                    .Where(x => x.Email == input.Email && x.TaskId == myTask.Id.ToString())
                    .ToList().FirstOrDefault();

            if (taskEmail == null)
            {
                throw new UserFriendlyException(L["Exception:No Permission"]);
            }

            var Inputs = new Dictionary<string, string>
            {
                { "Reason", $"{input.Reason}" },
                { "TriggeredBy", $"{input.Email}" }
            };

            var affectedWorkflows = await _signaler.TriggerSignalAsync(myTask.RejectSignal, Inputs, myTask.WorkflowInstanceId).ToList();

            var signal = new SignalModel(myTask.RejectSignal, myTask.WorkflowInstanceId);
            await _mediator.Publish(new HttpTriggeredSignal(signal, affectedWorkflows));

            myTask.Status = W2TaskStatus.Reject;
            myTask.UpdatedBy = input.Email;
            myTask.Reason = input.Reason;

            await _taskRepository.UpdateAsync(myTask);

            return new Dictionary<string, string> {
                { "id", input.Id.ToString() },
                { "message", "Reject Request Successfully!" }
            };
        }
    }
}
