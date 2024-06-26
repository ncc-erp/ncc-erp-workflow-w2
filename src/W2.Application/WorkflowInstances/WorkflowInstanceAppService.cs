﻿using Elsa;
using Elsa.Activities.Http.Events;
using Elsa.Activities.Signaling.Models;
using Elsa.Activities.Signaling.Services;
using Elsa.Models;
using Elsa.Persistence;
using Elsa.Persistence.Specifications;
using Elsa.Persistence.Specifications.WorkflowInstances;
using Elsa.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Namotion.Reflection;
using NetBox.Extensions;
using Newtonsoft.Json;
using Open.Linq.AsyncExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Http;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Emailing;
using Volo.Abp.Identity;
using Volo.Abp.TextTemplating;
using Volo.Abp.Uow;
using Volo.Abp.Users;
using W2.ExternalResources;
using W2.Permissions;
using W2.Specifications;
using W2.TaskActions;
using W2.Tasks;
using W2.Utils;
using W2.WorkflowDefinitions;
using static IdentityServer4.Models.IdentityResources;

namespace W2.WorkflowInstances
{
    [Authorize(W2Permissions.WorkflowManagementWorkflowInstances)]
    public class WorkflowInstanceAppService : W2AppService, IWorkflowInstanceAppService
    {
        private readonly IWorkflowLaunchpad _workflowLaunchpad;
        private readonly IRepository<WorkflowInstanceStarter, Guid> _instanceStarterRepository;
        private readonly IRepository<W2Task, Guid> _taskRepository;
        private readonly IRepository<W2TaskActions, Guid> _taskActionsRepository;
        private readonly IWorkflowInstanceStore _workflowInstanceStore;
        private readonly IWorkflowDefinitionStore _workflowDefinitionStore;
        private readonly IWorkflowInstanceCanceller _canceller;
        private readonly IWorkflowInstanceDeleter _workflowInstanceDeleter;
        private readonly ILogger<WorkflowInstanceAppService> _logger;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IIdentityUserRepository _userRepository;
        private readonly IAntClientApi _antClientApi;
        private readonly IConfiguration _configuration;
        private readonly IDataFilter _dataFilter;
        private readonly ICurrentUser _currentUser;
        private readonly IRepository<WorkflowCustomInputDefinition, Guid> _workflowCustomInputDefinitionRepository;
        private readonly IExternalResourceAppService _externalResourceAppService;

        public WorkflowInstanceAppService(IWorkflowLaunchpad workflowLaunchpad,
            IRepository<WorkflowInstanceStarter, Guid> instanceStarterRepository,
            IRepository<W2Task, Guid> taskRepository,
                        IRepository<W2TaskActions, Guid> taskActionsRepository,
            IWorkflowInstanceStore workflowInstanceStore,
            IWorkflowDefinitionStore workflowDefinitionStore,
            IWorkflowInstanceCanceller canceller,
            IWorkflowInstanceDeleter workflowInstanceDeleter,
            ILogger<WorkflowInstanceAppService> logger,
            IUnitOfWorkManager unitOfWorkManager,
            IIdentityUserRepository userRepository,
            IAntClientApi antClientApi,
            IConfiguration configuration,
            IDataFilter dataFilter,
            ICurrentUser currentUser,
            IRepository<WorkflowCustomInputDefinition, Guid> workflowCustomInputDefinitionRepository,
            IExternalResourceAppService externalResourceAppService
            )
        {
            _workflowLaunchpad = workflowLaunchpad;
            _instanceStarterRepository = instanceStarterRepository;
            _workflowInstanceStore = workflowInstanceStore;
            _workflowDefinitionStore = workflowDefinitionStore;
            _canceller = canceller;
            _workflowInstanceDeleter = workflowInstanceDeleter;
            _logger = logger;
            _unitOfWorkManager = unitOfWorkManager;
            _userRepository = userRepository;
            _antClientApi = antClientApi;
            _configuration = configuration;
            _taskRepository = taskRepository;
            _taskActionsRepository = taskActionsRepository;
            _dataFilter = dataFilter;
            _currentUser = currentUser;
            _workflowCustomInputDefinitionRepository = workflowCustomInputDefinitionRepository;
            _externalResourceAppService = externalResourceAppService;
        }

        public async Task<string> CancelAsync(string id)
        {
            var workflowInstance = await _workflowInstanceStore.FindByIdAsync(id);
            var isAdmin = _currentUser.IsInRole("admin");

            var workflowInstanceStarter = await _instanceStarterRepository.FirstOrDefaultAsync(x => x.WorkflowInstanceId == id);
            if (workflowInstanceStarter == null)
            {
                throw new UserFriendlyException(L["Cancel Failed: workflowInstanceStarter Is Not Found!"]);
            }
            else if (!isAdmin && workflowInstanceStarter.Status != WorkflowInstancesStatus.Pending)
            {
                throw new UserFriendlyException(L[" Cannot cancel a request if status is not Pending!"]);
            }

            var myTasks = await _taskRepository.GetListAsync(x => x.WorkflowInstanceId == id);

            // skip if current user is a admin
            var hasAnyApproved = isAdmin ? false : myTasks.Any(task => task.Status != W2TaskStatus.Pending);

            var taskActions = await _taskActionsRepository.GetListAsync(x => myTasks.Select(task => task.Id.ToString()).Contains(x.TaskId) && x.Status != W2TaskActionsStatus.Pending);

            if (hasAnyApproved == true || (!isAdmin && (taskActions != null && taskActions.Count > 0)))
            {
                throw new UserFriendlyException(L["Cancel Failed: No Permission!"]);
            }

            // Only allow workflow has pending or failed status to cancel
            if (workflowInstance == null || (workflowInstance.WorkflowStatus != WorkflowStatus.Suspended && workflowInstance.WorkflowStatus != WorkflowStatus.Faulted))
            {
                throw new UserFriendlyException(L["Cancel Failed: Workflow Is Not Valid!"]);
            }

            workflowInstanceStarter.Status = WorkflowInstancesStatus.Canceled;
            await _instanceStarterRepository.UpdateAsync(workflowInstanceStarter);

            var tasks = (await _taskRepository.GetListAsync()).Where(x => x.WorkflowInstanceId == id && x.Status == W2TaskStatus.Pending).ToList();
            if (tasks != null && tasks.Count > 0)
            {
                foreach (var task in tasks)
                {
                    task.Status = W2TaskStatus.Cancel;
                    task.Reason = "Workflow being canceled";
                }

                await _taskRepository.UpdateManyAsync(tasks);
            }

            var cancelResult = await _canceller.CancelAsync(id);

            switch (cancelResult.Status)
            {
                case CancelWorkflowInstanceResultStatus.NotFound:
                    throw new UserFriendlyException(L["Exception:InstanceNotFound"]);
                case CancelWorkflowInstanceResultStatus.InvalidStatus:
                    throw new UserFriendlyException(L["Exception:CancelInstanceInvalidStatus", cancelResult.WorkflowInstance!.WorkflowStatus]);
            }

            return "Cancel Request workflow successful";
        }

        [Authorize(W2Permissions.WorkflowManagementWorkflowInstancesCreate)]
        public async Task<string> CreateNewInstanceAsync(CreateNewWorkflowInstanceDto input)
        {
            var startableWorkflow = await _workflowLaunchpad.FindStartableWorkflowAsync(input.WorkflowDefinitionId, tenantId: CurrentTenantStrId);

            if (startableWorkflow == null)
            {
                throw new UserFriendlyException(L["Exception:NoStartableWorkflowFound"]);
            }

            var httpRequestModel = GetHttpRequestModel(nameof(HttpMethod.Post), input.Input);

            var executionResult = await _workflowLaunchpad.ExecuteStartableWorkflowAsync(startableWorkflow, new WorkflowInput(httpRequestModel));

            var instance = executionResult.WorkflowInstance;
            using (var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false))
            {
                var workflowInstanceStarter = new WorkflowInstanceStarter
                {
                    WorkflowInstanceId = instance.Id,
                    WorkflowDefinitionId = instance.DefinitionId,
                    WorkflowDefinitionVersionId = instance.DefinitionVersionId,
                    Input = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(input.Input))
                };

                await _instanceStarterRepository.InsertAsync(workflowInstanceStarter);
                await uow.CompleteAsync();

                _logger.LogInformation("Saved changes to database");
            }

            return instance.Id;
        }

        [AllowAnonymous]
        public async Task<WorkflowInstanceDto> GetByIdAsync(string id)
        {
            var specification = new WorkflowInstanceIdsSpecification(new[] { id });
            var instance = await _workflowInstanceStore.FindAsync(specification);
            if (instance == null)
            {
                throw new UserFriendlyException(L["Exception:InstanceNotFound"]);
            }

            var instanceDto = ObjectMapper.Map<WorkflowInstance, WorkflowInstanceDto>(instance);
            _logger.LogInformation("Fetch WorkflowInstanceDto begin");
            var workflowInstanceStarter = await _instanceStarterRepository.FirstOrDefaultAsync(x => x.WorkflowInstanceId == id);
            if (workflowInstanceStarter != null)
            {
                instanceDto.CreatorId = workflowInstanceStarter.CreatorId;
            }
            else
            {
                _logger.LogInformation("Workflow not found");
            }

            return instanceDto;
        }

        // todo refactor/ new logic
        [AllowAnonymous]
        public async Task<WorkflowStatusDto> GetWfhStatusAsync([Required] string email,
            [Required]
            [RegularExpression("^\\d{4}\\-(0[1-9]|1[012])\\-(0[1-9]|[12][0-9]|3[01])$", ErrorMessage = "Invalid Date yyyy-MM-dd")]
            string date)
        {// date "dd/MM/yyyy"
            var dateArray = date.Split("-");
            var dateDb = $"{dateArray[2]}/{dateArray[1]}/{dateArray[0]}";
            string defaultWFHDefinitionsId = _configuration.GetValue<string>("DefaultWFHDefinitionsId");

            var specification = Specification<WorkflowInstance>.Identity;
            if (CurrentTenant.IsAvailable)
            {
                specification = specification.WithTenant(CurrentTenantStrId);
            }
            specification = specification.WithWorkflowDefinition(defaultWFHDefinitionsId);

            var instances = await _workflowInstanceStore.FindManyAsync(specification);
            var workflowDefinitions = (await _workflowDefinitionStore.FindManyAsync(
                new ListAllWorkflowDefinitionsSpecification(CurrentTenantStrId, instances.Select(i => i.DefinitionId).ToArray())
            )).ToList();

            var instancesIds = instances.Select(x => x.Id);
            var workflowInstanceStarters = new List<WorkflowInstanceStarter>();

            var requestUser = await _userRepository.FindByNormalizedEmailAsync(email.ToUpper());

            var allWorkflowInstanceStarters = await AsyncExecuter.ToListAsync(await _instanceStarterRepository.GetQueryableAsync());
            workflowInstanceStarters = allWorkflowInstanceStarters
                .Where(x => instancesIds.Contains(x.WorkflowInstanceId) && x.Input != null && x.Input.GetValueOrDefault("Dates") != null && x.Input.GetValueOrDefault("Dates").Contains(dateDb) && x.CreatorId == requestUser.Id)
                .ToList();

            instances = await AsyncExecuter.ToListAsync(
                workflowInstanceStarters.Join(instances, x => x.WorkflowInstanceId, x => x.Id, (WorkflowInstanceStarter, WorkflowInstance) => new
                {
                    WorkflowInstanceStarter,
                    WorkflowInstance
                })
                .AsQueryable()
                .Select(x => x.WorkflowInstance)
            );

            var result = new WorkflowStatusDto();

            foreach (var instance in instances)
            {
                if (result.Email != null && result.Status == 1)
                {
                    continue;
                }
                var workflowInstanceStarter = workflowInstanceStarters.FirstOrDefault(x => x.WorkflowInstanceId == instance.Id);
                var workflowInstanceDto = ObjectMapper.Map<WorkflowInstance, WorkflowStatusDto>(instance);

                if (instance.WorkflowStatus == WorkflowStatus.Finished)
                {
                    var workflowDefinition = workflowDefinitions.FirstOrDefault(x => x.DefinitionId == instance.DefinitionId);
                    var lastExecutedActivity = workflowDefinition.Activities.FirstOrDefault(x => x.ActivityId == instance.LastExecutedActivityId);
                    workflowInstanceDto.Status = GetFinalStatus(lastExecutedActivity) == "Approved" ? 1 : 2;
                }

                workflowInstanceDto.Email = email;
                workflowInstanceDto.Status = (int)workflowInstanceStarter.Status;
                workflowInstanceDto.Date = date; //  workflowInstanceStarter.Input.GetValue("Dates");
                result = workflowInstanceDto;
            }
            if (result.Email == null)
            {
                var newInstanceError = new WorkflowStatusDto
                {
                    Email = email,
                    Date = date,
                    Status = -1
                };
                return newInstanceError;
            };

            return result;
        }

        public async Task<PagedResultDto<WFHDto>> GetWfhListAsync(ListAllWFHRequestInput input)
        {
            string defaultWFHDefinitionsId = _configuration.GetValue<string>("DefaultWFHDefinitionsId");

            var specification = Specification<WorkflowInstance>.Identity;
            specification = specification.WithWorkflowDefinition(defaultWFHDefinitionsId)
                .WithStatus(WorkflowStatus.Finished);

            var instances = (await _workflowInstanceStore.FindManyAsync(specification));
            var workflowDefinitions = (await _workflowDefinitionStore.FindManyAsync(
                new ListAllWorkflowDefinitionsSpecification(CurrentTenantStrId, instances.Select(i => i.DefinitionId).ToArray())
            )).ToList();

            instances = instances.Where(instance =>
            {
                var workflowDefinition = workflowDefinitions.FirstOrDefault(x => x.DefinitionId == instance.DefinitionId);
                var lastExecutedActivity = workflowDefinition.Activities.FirstOrDefault(x => x.ActivityId == instance.LastExecutedActivityId);

                return GetFinalStatus(lastExecutedActivity) == "Approved";
            });

            var instancesIds = instances.Select(x => x.Id);
            var tasks = (await _taskRepository.GetListAsync());
            var workflowInstanceStarters = new List<WorkflowInstanceStarter>();
            var workflowInstanceStartersQuery = await _instanceStarterRepository.GetQueryableAsync();
            workflowInstanceStartersQuery = workflowInstanceStartersQuery
                .Where(x => instancesIds.Contains(x.WorkflowInstanceId));
            workflowInstanceStarters = await AsyncExecuter.ToListAsync(workflowInstanceStartersQuery);

            var instancesQuery = workflowInstanceStarters
                .Join(instances, x => x.WorkflowInstanceId, x => x.Id,
                (WorkflowInstanceStarter, WorkflowInstance) => new
                {
                    WorkflowInstanceStarter,
                    WorkflowInstance
                })
                .GroupJoin(tasks, x => x.WorkflowInstance.Id, x => x.WorkflowInstanceId,
                (joinedEntities, W2task) => new
                {
                    joinedEntities.WorkflowInstanceStarter,
                    joinedEntities.WorkflowInstance,
                    W2task = W2task.FirstOrDefault()
                })
                .AsQueryable();

            var requestUserIds = workflowInstanceStarters.Select(x => (Guid)x.CreatorId);
            var totalCount = (await _userRepository.GetListAsync())
                .Where(x => x.Id.IsIn(requestUserIds))
                .Count();

            var query = (await _userRepository.GetListAsync())
                .Where(x => x.Id.IsIn(requestUserIds))
                .AsQueryable();

            if (!string.IsNullOrEmpty(input.KeySearch))
            {
                query = query.Where(x => x.Email.Contains(input.KeySearch));
            }

            var requestUsers = query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToList();

            var WFHList = new List<WFHDto>();
            foreach (var requestUser in requestUsers)
            {
                var totalDays = 0;
                List<string> totalDaysStr = new List<string>();
                var totalPosts = 0;
                List<string> totalPostStr = new List<string>();
                List<object> newPostList = new List<object>();

                var workflowInstanceStarterList = workflowInstanceStarters.Where(x => x.CreatorId == requestUser.Id).ToList();

                foreach (var workflowInstanceStarter in workflowInstanceStarterList)
                {
                    var workflowInstance = instances.FirstOrDefault(x => x.Id == workflowInstanceStarter.WorkflowInstanceId);

                    var data = workflowInstance.Variables.Data;
                    foreach (KeyValuePair<string, object> kvp in data)
                    {
                        var value = kvp.Value;
                        if (value is Dictionary<string, string> dateDictionary && dateDictionary.ContainsKey("Dates"))
                        {
                            string dateStr = dateDictionary["Dates"];
                            char[] separator = new char[] { ',' };
                            string[] dateArray = dateStr.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                            if (input.StartDate != DateTime.MinValue && input.EndDate != DateTime.MinValue)
                            {
                                foreach (string dateString in dateArray)
                                {
                                    if (DateTime.TryParseExact(dateString, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                                    {
                                        if (date >= input.StartDate && date <= input.EndDate)
                                        {
                                            totalDays++;
                                            totalDaysStr.Add(date.ToString("dd/MM/yyyy"));
                                        }
                                    }
                                }
                            }
                            else
                            {
                                totalDays += dateArray.Length;
                                totalDaysStr.AddRange(dateArray);
                            }
                        }
                    }
                }

                if (totalDays == 0) {
                    continue;
                }

                var users = await GetUserInfoBySlug(ConvertEmailToSlug(requestUser.Email));
                List<PostItem> posts = (users.Count > 0) ? await GetPostByAuthor(users[0].id) : new List<PostItem>();

                foreach (var post in posts)
                {
                    if (DateTime.TryParseExact(post.date, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date) &&
                        (input.StartDate == DateTime.MinValue || (date >= input.StartDate && date <= input.EndDate)))
                    {
                        totalPosts++;
                        totalPostStr.Add(date.ToString("dd/MM/yyyy"));
                        newPostList.Add(post);
                    }
                }

                var sortedRequestDates = SortAndRemoveDuplicates(totalDaysStr);
                var sortedPostsDates = Sort(totalPostStr);

                int totalMissingPosts = sortedRequestDates.Count;
                foreach (string request in sortedRequestDates)
                {
                    for (int i = 0; i < sortedPostsDates.Count; i++)
                    {
                        if (IsDateGreaterOrEqual(sortedPostsDates[i], request))
                        {
                            totalMissingPosts--;
                            sortedPostsDates.RemoveAt(i);
                            break;
                        }
                    }
                }

                var wfhDto = new WFHDto
                {
                    email = requestUser.Email,
                    posts = newPostList,
                    totalPosts = totalPosts,
                    requests = workflowInstanceStarterList,
                    requestDates = sortedRequestDates,
                    totalDays = sortedRequestDates.Count,
                    totalMissingPosts = totalMissingPosts,
                };
                WFHList.Add(wfhDto);
            }

            List<WFHDto> sortedList = SortWFHDtos(WFHList, input.Sorting);
            return new PagedResultDto<WFHDto>(totalCount, sortedList);
        }


        public async Task<PagedResultDto<WorkflowInstanceDto>> ListAsync(ListAllWorkflowInstanceInput input)
        {
            var specialStatus = new string[] { "approved", "rejected" };
            var isAdmin = _currentUser.IsInRole("admin");
            // hot fix load 
            var usersQuery = await _userRepository.GetListAsync();
            
            var workflowInstanceStartersOptQuery = await _instanceStarterRepository.GetQueryableAsync();
            if (!string.IsNullOrWhiteSpace(input?.WorkflowDefinitionId))
            {
                workflowInstanceStartersOptQuery = workflowInstanceStartersOptQuery.Where(x => x.WorkflowDefinitionId == input.WorkflowDefinitionId);
            }
            if (!isAdmin)
            {
                workflowInstanceStartersOptQuery = workflowInstanceStartersOptQuery.Where(x => x.CreatorId == _currentUser.Id);
            }

            // If search by user and the user is admin
            if (!string.IsNullOrWhiteSpace(input?.RequestUser) && isAdmin)
            {
                workflowInstanceStartersOptQuery = workflowInstanceStartersOptQuery.Where(x => x.CreatorId == Guid.Parse(input.RequestUser));
            }

            if (!string.IsNullOrWhiteSpace(input?.EmailRequest) && isAdmin)
            {
                string emailRequest = input.EmailRequest.Trim().ToLowerInvariant();
                usersQuery = usersQuery.Where(x => x.Email.ToLowerInvariant().Contains(input?.EmailRequest) && !x.IsDeleted).ToList();
            }

            List<Guid> creatorIds = usersQuery.Select(x => x.Id).ToList();

            // get request by creatorIds
            workflowInstanceStartersOptQuery = workflowInstanceStartersOptQuery.Where(x => creatorIds.Contains((Guid)x.CreatorId));

            if (!string.IsNullOrWhiteSpace(input?.Status))
            {
                WorkflowInstancesStatus status = WorkflowInstancesStatus.Pending;
                switch(input.Status)
                {
                    case "Approved":
                        status = WorkflowInstancesStatus.Approved;
                        break;
                    case "Rejected":
                        status = WorkflowInstancesStatus.Rejected; 
                        break;
                    case "Pending":
                        status = WorkflowInstancesStatus.Pending;
                        break;
                    default:
                        status = WorkflowInstancesStatus.Canceled;
                        break;

                }
                workflowInstanceStartersOptQuery = workflowInstanceStartersOptQuery.Where(x => x.Status == status);
            }
            var workflowInstanceStartersOptQueryLimit = workflowInstanceStartersOptQuery
                .OrderBy(input.Sorting.ReplaceFirst("createdAt", "CreationTime").ToLower()) // todo apply sort options
                .Skip(input.SkipCount).Take(input.MaxResultCount);
            var instanceIds = workflowInstanceStartersOptQueryLimit.Select(x => x.WorkflowInstanceId).ToArray();

            var specification = Specification<WorkflowInstance>.Identity;
            specification.And(new ListAllWorkflowInstancesSpecification(CurrentTenantStrId, instanceIds));
            //if (CurrentTenant.IsAvailable)
            //{
            //    specification = specification.WithTenant(CurrentTenantStrId);
            //}
            //if (!string.IsNullOrWhiteSpace(input?.WorkflowDefinitionId))
            //{
            //    specification = specification.WithWorkflowDefinition(input.WorkflowDefinitionId);
            //}
            //if (!string.IsNullOrWhiteSpace(input?.Status))
            //{
            //    if (!specialStatus.Contains(input.Status.ToLower()))
            //    {
            //        specification = specification.WithStatus((WorkflowStatus)Enum.Parse(typeof(WorkflowStatus), input.Status, true));
            //    }
            //    else
            //    {
            //        specification = specification.WithStatus(WorkflowStatus.Finished);
            //    }
            //}

            var instances = (await _workflowInstanceStore.FindManyAsync(new WorkflowInstanceIdsSpecification(instanceIds)));
            var workflowDefinitions = (await _workflowDefinitionStore.FindManyAsync(
                new ListAllWorkflowDefinitionsSpecification(CurrentTenantStrId, instances.Select(i => i.DefinitionId).ToArray())
            )).ToList();

            //if (specialStatus.Contains(input.Status.ToLower()))
            //{
            //    instances = instances.Where(instance =>
            //    {
            //        var workflowDefinition = workflowDefinitions.FirstOrDefault(x => x.DefinitionId == instance.DefinitionId);
            //        var lastExecutedActivity = workflowDefinition.Activities.FirstOrDefault(x => x.ActivityId == instance.LastExecutedActivityId);

            //        return GetFinalStatus(lastExecutedActivity) == input.Status;
            //    });
            //}

            //var instancesIds = instances.Select(x => x.Id);
            var tasks = await _taskRepository.GetQueryableAsync();
            tasks = tasks.Where(x => instanceIds.Contains(x.WorkflowInstanceId));
            var workflowInstanceStarters = new List<WorkflowInstanceStarter>();
            var workflowInstanceStartersQuery = workflowInstanceStartersOptQueryLimit; //  await _instanceStarterRepository.GetQueryableAsync();

            //if (!await AuthorizationService.IsGrantedAsync(W2Permissions.WorkflowManagementWorkflowInstancesViewAll))
            //{
            //    workflowInstanceStartersQuery = workflowInstanceStartersQuery
            //                    .Where(x => instancesIds.Contains(x.WorkflowInstanceId) && x.CreatorId == CurrentUser.Id);
            //}
            //else
            //{
            //    workflowInstanceStartersQuery = workflowInstanceStartersQuery
            //                    .Where(x => instancesIds.Contains(x.WorkflowInstanceId));
            //}

            workflowInstanceStarters = await AsyncExecuter.ToListAsync(workflowInstanceStartersQuery);

            var requestUsers = usersQuery;

            var instancesQuery = (from w in workflowInstanceStartersQuery.ToList()
                                  from i in instances.Where(i => i.Id == w.WorkflowInstanceId).DefaultIfEmpty()
                                  from t in tasks.Where(t => w.WorkflowInstanceId == t.WorkflowInstanceId).DefaultIfEmpty()
                                  from d in workflowDefinitions.Where(d => i?.DefinitionId == d.DefinitionId).DefaultIfEmpty()
                                  from u in requestUsers.Where(u => u.Id == w.CreatorId).DefaultIfEmpty()
                                  select new
                                  {
                                      WorkflowInstanceStarter = w,
                                      WorkflowInstance = i,
                                      W2task = t,
                                      Definition = d,
                                      User = u
                                  })
                                  .GroupBy(p => p.WorkflowInstanceStarter.Id)
                                  .Select(g => g.First())
                .ToList();
            var totalCount = workflowInstanceStartersOptQuery.Count();
            var totalResults = instancesQuery.Select(x => new
                {
                    instance = x.WorkflowInstance,
                    task = x.W2task,
                    instanceStarter = x.WorkflowInstanceStarter,
                    definition = x.Definition,
                    user = x.User
                }).ToList();

            var totalResultsAfterMapping = new List<WorkflowInstanceDto>();
            var stakeHolderEmails = new Dictionary<string, string>();
            // get all defines
            var listDefineIds = totalResults.Select(x => x.definition.DefinitionId).ToList();
            var allDefines = (await _workflowCustomInputDefinitionRepository.GetQueryableAsync())
                .Where(i => listDefineIds.Contains(i.WorkflowDefinitionId))
                .ToDictionary(x => x.WorkflowDefinitionId, x => x.PropertyDefinitions.Where(p => p.IsTitle).FirstOrDefault());

            foreach (var res in totalResults)
            {
                var instance = res.instance;
                if (instance == null)
                {
                    totalResultsAfterMapping.Add(new WorkflowInstanceDto
                    {
                        WorkflowDefinitionId = res.instanceStarter.WorkflowDefinitionId,
                        CreatedAt = res.instanceStarter.CreationTime,
                        CreatorId = res.instanceStarter.CreatorId,
                        Status = res.instanceStarter.Status.ToString(),
                        WorkflowDefinitionDisplayName = res.definition == null ? "NotFound" : res.definition.Name,
                        Id = res.instanceStarter.WorkflowInstanceId,
                        UserRequestName = res.user?.Name,
                        CurrentStates = new List<string>(),
                        StakeHolders = new List<string>(),
                        LastExecutedAt = res.instanceStarter.CreationTime
                    });
                    continue;
                }
                var task = res.task;

                var workflowDefinition = workflowDefinitions.FirstOrDefault(x => x.DefinitionId == instance.DefinitionId);
                var workflowInstanceDto = ObjectMapper.Map<WorkflowInstance, WorkflowInstanceDto>(instance);
                workflowInstanceDto.WorkflowDefinitionDisplayName = workflowDefinition?.DisplayName ?? "Not Found";
                workflowInstanceDto.StakeHolders = new List<string>();
                workflowInstanceDto.CurrentStates = new List<string>();

                workflowInstanceDto.Status = res.instanceStarter.Status.ToString();
                //if (instance.WorkflowStatus == WorkflowStatus.Finished)
                //{
                //    var lastExecutedActivity = workflowDefinition.Activities.FirstOrDefault(x => x.ActivityId == instance.LastExecutedActivityId);
                //    workflowInstanceDto.Status = GetFinalStatus(lastExecutedActivity);
                //}

                var workflowInstanceStarter = workflowInstanceStarters.FirstOrDefault(x => x.WorkflowInstanceId == instance.Id);
                if (workflowInstanceStarter is not null)
                {
                    using (_dataFilter.Disable<ISoftDelete>())
                    {
                        var identityUser = requestUsers.FirstOrDefault(x => x.Id == workflowInstanceStarter.CreatorId.Value);

                        if (identityUser != null && !stakeHolderEmails.ContainsKey(identityUser.Email))
                        {
                            stakeHolderEmails.Add(identityUser.Email, identityUser.Name);
                        }
                        if (identityUser != null)
                        {
                            workflowInstanceDto.UserRequestName = stakeHolderEmails[identityUser.Email];
                        }
                        else
                        {
                            workflowInstanceDto.UserRequestName = "[Deleted]";
                        }
                    }
                }

                var blockingActivityIds = instance.BlockingActivities.Select(x => x.ActivityId);
                if (allDefines.ContainsKey(workflowDefinition.DefinitionId))
                {
                    var titleFiled = allDefines.GetItem(workflowDefinition.DefinitionId);

                    workflowInstanceStarter.Input.Add("RequestUser", workflowInstanceDto.UserRequestName);
                    var title = TitleTemplateParser.ParseTitleTemplateToString(titleFiled.TitleTemplate, workflowInstanceStarter.Input);
                    workflowInstanceDto.ShortTitle = title.IsNullOrEmpty() ? workflowInstanceStarter.Input.GetItem(titleFiled.Name) : title;
                    //workflowInstanceDto.ShortTitle = workflowInstanceStarter.Input.GetItem(titleFiled.Name);
                }
                foreach (var blockingActitvity in instance.BlockingActivities)
                {
                    var connection = workflowDefinition.Connections.FirstOrDefault(x => x.TargetActivityId == blockingActitvity.ActivityId);

                    var parentActivity = workflowDefinition.Activities.FirstOrDefault(x => x.ActivityId == connection?.SourceActivityId);
                    if (parentActivity?.Type == "Fork")
                    {
                        var childNodes = workflowDefinition.Connections.Where(x => x.SourceActivityId == parentActivity.ActivityId && instance.ActivityData.ContainsKey(x.TargetActivityId))
                                                                       .Select(x => x.TargetActivityId);
                        if (!childNodes.All(x => blockingActivityIds.Contains(x)))
                        {
                            continue;
                        }

                        if (!workflowInstanceDto.CurrentStates.Contains(parentActivity.DisplayName) && task != null)
                        {
                            workflowInstanceDto.CurrentStates.Add(parentActivity.DisplayName);
                        }

                        connection = workflowDefinition.Connections.FirstOrDefault(x => x.TargetActivityId == parentActivity.ActivityId);

                        var parentForkActivity = workflowDefinition.Activities.FirstOrDefault(x => x.ActivityId == connection.SourceActivityId);

                        instance.ActivityData.TryGetValue(parentForkActivity.ActivityId, out var data);
                        while (data != null && !data.ContainsKey("To") && parentForkActivity != null)
                        {
                            connection = workflowDefinition.Connections.FirstOrDefault(x => x.TargetActivityId == parentForkActivity.ActivityId);
                            parentForkActivity = workflowDefinition.Activities.FirstOrDefault(x => x.ActivityId == connection.SourceActivityId);

                            instance.ActivityData.TryGetValue(parentForkActivity.ActivityId, out data);
                        }
                        if (data != null)
                        {

                            string key = data.ContainsKey("AssignTo") && data["AssignTo"] is List<string> dataList && dataList.Count > 0 ? "AssignTo" : data.ContainsKey("To") ? "To" : null;

                            if (key != null)
                            {
                                foreach (var email in (List<string>)data[key])
                                {
                                    string stakeHolderName = string.Empty;
                                    switch (email)
                                    {
                                        case "it@ncc.asia":
                                            stakeHolderName = "IT Department";
                                            break;
                                        case "sale@ncc.asia":
                                            stakeHolderName = "Sale Department";
                                            break;
                                        default:
                                            if (!stakeHolderEmails.ContainsKey(email))
                                            {
                                                var user = await _userRepository.FindByNormalizedEmailAsync(email.ToUpper());
                                                stakeHolderEmails.Add(email, user?.Name);
                                            }
                                            stakeHolderName = stakeHolderEmails[email];
                                            break;
                                    }

                                    if (!workflowInstanceDto.StakeHolders.Contains(stakeHolderName))
                                    {
                                        workflowInstanceDto.StakeHolders.Add(stakeHolderName);
                                    }
                                }
                            }
                        }
                    }
                }

                totalResultsAfterMapping.Add(workflowInstanceDto);
            }

            return new PagedResultDto<WorkflowInstanceDto>(totalCount, totalResultsAfterMapping);
        }

        private string GetFinalStatus(ActivityDefinition activityDefinition)
        {
            return activityDefinition == null ? "Finished" : (activityDefinition.DisplayName.ToLower().Contains("reject") ? "Rejected" : "Approved");
        }

        private bool IsDateGreaterOrEqual(string dateA, string dateB)
        {
            DateTime dateTimeA = DateTime.ParseExact(dateA, "dd/MM/yyyy", null);
            DateTime dateTimeB = DateTime.ParseExact(dateB, "dd/MM/yyyy", null);
            return dateTimeA >= dateTimeB;
        }

        private List<string> SortAndRemoveDuplicates(List<string> dates)
        {
            HashSet<string> uniqueDates = new HashSet<string>(dates);
            return uniqueDates.OrderBy(date => DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture)).ToList();
        }

        private List<string> Sort(List<string> dates)
        {
            return dates.OrderBy(date => DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture)).ToList();
        }

        private string NormalizeSortingString(string sorting)
        {
            if (sorting.IsNullOrEmpty())
            {
                return "WorkflowInstance.CreatedAt DESC";
            }

            var sortingPart = sorting.Trim().Split(' ');
            var property = sortingPart[0].ToLower();
            var direction = sortingPart[1].ToLower();
            switch (property)
            {
                case "createdat":
                    if (direction == "asc")
                    {
                        return "WorkflowInstance.CreatedAt ASC";
                    }
                    return "WorkflowInstance.CreatedAt DESC";
                case "lastexecutedat":
                    if (direction == "asc")
                    {
                        return "WorkflowInstance.LastExecutedAt ASC";
                    }
                    return "WorkflowInstance.LastExecutedAt DESC";
            }

            return "WorkflowInstance.CreatedAt DESC";
        }

        public List<WFHDto> SortWFHDtos(List<WFHDto> wfhList, string sortExpression)
        {
            if (string.IsNullOrWhiteSpace(sortExpression))
            {
                return wfhList;
            }

            string[] parts = sortExpression.Split(' ');
            string fieldName = parts[0];
            string sortOrder = parts.Length > 1 ? parts[1] : "asc";

            IOrderedEnumerable<WFHDto> sortedList = null;

            switch (fieldName)
            {
                case "email":
                    sortedList = sortOrder.ToLower() == "asc"
                        ? wfhList.OrderBy(dto => dto.email)
                        : wfhList.OrderByDescending(dto => dto.email);
                    break;
                case "totalPosts":
                    sortedList = sortOrder.ToLower() == "asc"
                        ? wfhList.OrderBy(dto => dto.totalPosts)
                        : wfhList.OrderByDescending(dto => dto.totalPosts);
                    break;
                case "totalDays":
                    sortedList = sortOrder.ToLower() == "asc"
                        ? wfhList.OrderBy(dto => dto.totalDays)
                        : wfhList.OrderByDescending(dto => dto.totalDays);
                    break;
                case "totalMissingPosts":
                    sortedList = sortOrder.ToLower() == "asc"
                        ? wfhList.OrderBy(dto => dto.totalMissingPosts)
                        : wfhList.OrderByDescending(dto => dto.totalMissingPosts);
                    break;
                default:
                    return wfhList;
            }

            return sortedList.ToList();
        }

        private string ConvertEmailToSlug(string email)
        {
            string cleanedUsername = email.Split("@")[0].Replace(".", "-");

            return cleanedUsername;
        }

        public async Task<List<UserInfoBySlug>> GetUserInfoBySlug(string slug)
        {
            var response = await _antClientApi.GetUsersBySlugAsync(slug);
            var users = response != null ? response
                .OrderBy(x => x.id)
                .ToList() : new List<UserInfoBySlug>();
            return users;
        }

        public async Task<List<PostItem>> GetPostByAuthor(int author)
        {
            var response = await _antClientApi.GetPostsByUserIdAsync(author);
            var posts = response != null ? response
                .OrderBy(x => x.id)
                .ToList() : new List<PostItem>();
            return posts;
        }

        public async Task<WorkflowInstanceDetailDto> GetDetailByIdAsync(string id)
        {
            var workflowInstance = await _workflowInstanceStore.FindByIdAsync(id);

            var workflowDefinitions = (await _workflowDefinitionStore.FindManyAsync(
                new ListAllWorkflowDefinitionsSpecification(CurrentTenantStrId, new string[] { workflowInstance?.DefinitionId }))).FirstOrDefault();

            Dictionary<string, dynamic> input = new Dictionary<string, dynamic>();

            if (workflowInstance != null)
            {
                input = (Dictionary<string, dynamic>)(workflowInstance?.Variables.Data);
            }
            else
            {
                var requestUserFormat = new Dictionary<string, dynamic>();
                var workflowInstanceStarter = await _instanceStarterRepository.FirstOrDefaultAsync(x => x.WorkflowInstanceId == id);

                var requestIndentityUser = await _userRepository.FindAsync((Guid)workflowInstanceStarter.CreatorId);

                var branchResult = await _externalResourceAppService.GetUserBranchInfoAsync(requestIndentityUser.Email);

                requestUserFormat.Add("id", requestIndentityUser?.Id);
                requestUserFormat.Add("email", requestIndentityUser?.Email);
                requestUserFormat.Add("name", requestIndentityUser?.Name);
                requestUserFormat.Add("headOfOfficeEmail", branchResult?.HeadOfOfficeEmail);
                requestUserFormat.Add("branchCode", branchResult?.Code);
                requestUserFormat.Add("branchName", branchResult?.DisplayName);

                input.Add("Request", workflowInstanceStarter.Input);
                input.Add("RequestUser", requestUserFormat);
            }

            var tasks = await _taskRepository.GetListAsync(x => x.WorkflowInstanceId == id);
            var requestTasks = ObjectMapper.Map<List<W2Task>, List<W2TasksDto>>(tasks);
            var workflowInstanceDetailDto = new WorkflowInstanceDetailDto
            {
                workInstanceId = id,
                tasks = requestTasks,
                input = input,
                typeRequest = workflowDefinitions != null ? workflowDefinitions.DisplayName : "[Deleted]",
            };

            return workflowInstanceDetailDto;
        }
    }
}
