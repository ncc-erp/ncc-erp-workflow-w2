using Elsa;
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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Open.Linq.AsyncExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Http;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Emailing;
using Volo.Abp.Identity;
using Volo.Abp.TextTemplating;
using Volo.Abp.Uow;
using W2.ExternalResources;
using W2.Permissions;
using W2.Specifications;
using W2.Tasks;

namespace W2.WorkflowInstances
{
    [Authorize(W2Permissions.WorkflowManagementWorkflowInstances)]
    public class WorkflowInstanceAppService : W2AppService, IWorkflowInstanceAppService
    {
        private readonly IWorkflowLaunchpad _workflowLaunchpad;
        private readonly IRepository<WorkflowInstanceStarter, Guid> _instanceStarterRepository;
        private readonly IRepository<W2Task, Guid> _taskRepository;
        private readonly IWorkflowInstanceStore _workflowInstanceStore;
        private readonly IWorkflowDefinitionStore _workflowDefinitionStore;
        private readonly IWorkflowInstanceCanceller _canceller;
        private readonly IWorkflowInstanceDeleter _workflowInstanceDeleter;
        private readonly ILogger<WorkflowInstanceAppService> _logger;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IIdentityUserRepository _userRepository;
        private readonly IAntClientApi _antClientApi;
        private readonly IConfiguration _configuration;
        public WorkflowInstanceAppService(IWorkflowLaunchpad workflowLaunchpad,
            IRepository<WorkflowInstanceStarter, Guid> instanceStarterRepository,
            IRepository<W2Task, Guid> taskRepository,
            IWorkflowInstanceStore workflowInstanceStore,
            IWorkflowDefinitionStore workflowDefinitionStore,
            IWorkflowInstanceCanceller canceller,
            IWorkflowInstanceDeleter workflowInstanceDeleter,
            ILogger<WorkflowInstanceAppService> logger,
            IUnitOfWorkManager unitOfWorkManager,
            IIdentityUserRepository userRepository,
            IAntClientApi antClientApi,
            IConfiguration configuration)
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
        }

        public async Task<string> CancelAsync(string id)
        {
            var workflowInstance = await _workflowInstanceStore.FindByIdAsync(id);

            // Only allow workflow has pending or failed status to cancel
            if (workflowInstance == null || workflowInstance.WorkflowStatus != WorkflowStatus.Suspended || workflowInstance.WorkflowStatus != WorkflowStatus.Faulted)
            {
                throw new UserFriendlyException(L["Exception:WorkflowNotValid"]);
            }

            var tasks =  (await _taskRepository.GetListAsync()).Where(x => x.WorkflowInstanceId == id && x.Status == W2TaskStatus.Pending).ToList();
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
                    Input = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(input.Input))
                };

                await _instanceStarterRepository.InsertAsync(workflowInstanceStarter);
                await uow.CompleteAsync();

                _logger.LogInformation("Saved changes to database");
            }

            return instance.Id;
        }

        public async Task<string> DeleteAsync(string id)
        {
            var workflowInstance = await _workflowInstanceStore.FindByIdAsync(id);

            // Only allow workflow has pending or faulted status to deleted
            if (workflowInstance == null || workflowInstance.WorkflowStatus != WorkflowStatus.Suspended || workflowInstance.WorkflowStatus != WorkflowStatus.Faulted)
            {
                throw new UserFriendlyException(L["Exception:WorkflowNotValid"]);
            }

            var tasks = (await _taskRepository.GetListAsync()).Where(x => x.WorkflowInstanceId == id && x.Status == W2TaskStatus.Pending).ToList();
            if (tasks != null && tasks.Count > 0)
            {
                await _taskRepository.DeleteManyAsync(tasks);
            }
            
            var result = await _workflowInstanceDeleter.DeleteAsync(id);
            if (result.Status == DeleteWorkflowInstanceResultStatus.NotFound)
            {
                throw new UserFriendlyException(L["Exception:InstanceNotFound"]);
            }

            return "Delete Request workflow successful";
        }

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
                .Where(x => instancesIds.Contains(x.WorkflowInstanceId) && x.Input != null && x.Input.GetValueOrDefault("Dates").Contains(dateDb) && x.CreatorId == requestUser.Id)
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
            specification = specification.WithWorkflowDefinition(defaultWFHDefinitionsId);

            var instances = (await _workflowInstanceStore.FindManyAsync(specification));

            var instancesIds = instances.Select(x => x.Id);
            var workflowInstanceStarters = new List<WorkflowInstanceStarter>();
            workflowInstanceStarters = await AsyncExecuter.ToListAsync((await _instanceStarterRepository.GetQueryableAsync())
                                .Where(x => instancesIds.Contains(x.WorkflowInstanceId)));

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
                .OrderBy(NormalizeSortingStringWFH(input.Sorting))
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToList();

            var WFHList = new List<WFHDto>();
            foreach (var requestUser in requestUsers)
            {
                var totalDays = 0;
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
                            totalDays += dateArray.Length;
                        }
                    }
                }
                
                var users = await GetUserInfoBySlug(ConvertEmailToSlug(requestUser.Email));
                List<PostItem> posts = (users.Count > 0) ? await GetPostByAuthor(users[0].id) : new List<PostItem>();

                var wfhDto = new WFHDto
                {
                    UserRequestName = requestUser.Email,
                    Posts = posts,
                    Totalposts = posts.Count,
                    Requests = workflowInstanceStarterList,
                    Totaldays = totalDays
                };
                WFHList.Add(wfhDto);
            }

            return new PagedResultDto<WFHDto>(totalCount, WFHList);
        }


        public async Task<PagedResultDto<WorkflowInstanceDto>> ListAsync(ListAllWorkflowInstanceInput input)
        {
            var specialStatus = new string[] { "approved", "rejected" };
            var specification = Specification<WorkflowInstance>.Identity;
            if (CurrentTenant.IsAvailable)
            {
                specification = specification.WithTenant(CurrentTenantStrId);
            }
            if (!string.IsNullOrWhiteSpace(input?.WorkflowDefinitionId))
            {
                specification = specification.WithWorkflowDefinition(input.WorkflowDefinitionId);
            }
            if (!string.IsNullOrWhiteSpace(input?.Status))
            {
                if (!specialStatus.Contains(input.Status.ToLower()))
                {
                    specification = specification.WithStatus((WorkflowStatus)Enum.Parse(typeof(WorkflowStatus), input.Status, true));
                }
                else
                {
                    specification = specification.WithStatus(WorkflowStatus.Finished);
                }
            }

            var instances = (await _workflowInstanceStore.FindManyAsync(specification));
            var workflowDefinitions = (await _workflowDefinitionStore.FindManyAsync(
                new ListAllWorkflowDefinitionsSpecification(CurrentTenantStrId, instances.Select(i => i.DefinitionId).ToArray())
            )).ToList();
            if (specialStatus.Contains(input.Status.ToLower()))
            {
                instances = instances.Where(instance =>
                {
                    var workflowDefinition = workflowDefinitions.FirstOrDefault(x => x.DefinitionId == instance.DefinitionId);
                    var lastExecutedActivity = workflowDefinition.Activities.FirstOrDefault(x => x.ActivityId == instance.LastExecutedActivityId);

                    return GetFinalStatus(lastExecutedActivity) == input.Status;
                });
            }

            var instancesIds = instances.Select(x => x.Id);
            var workflowInstanceStarters = new List<WorkflowInstanceStarter>();
            var workflowInstanceStartersQuery = await _instanceStarterRepository.GetQueryableAsync();

            if (!string.IsNullOrWhiteSpace(input?.RequestUser))
            {
                workflowInstanceStartersQuery = workflowInstanceStartersQuery.Where(x => x.CreatorId.ToString().Contains(input.RequestUser));
            }

            if (!await AuthorizationService.IsGrantedAsync(W2Permissions.WorkflowManagementWorkflowInstancesViewAll))
            {
                workflowInstanceStartersQuery = workflowInstanceStartersQuery
                                .Where(x => instancesIds.Contains(x.WorkflowInstanceId) && x.CreatorId == CurrentUser.Id);
            }
            else
            {
                workflowInstanceStartersQuery = workflowInstanceStartersQuery
                                .Where(x => instancesIds.Contains(x.WorkflowInstanceId));
            }

            workflowInstanceStarters = await AsyncExecuter.ToListAsync(workflowInstanceStartersQuery);
            var tasks = await _taskRepository.GetListAsync();
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

            if (!string.IsNullOrWhiteSpace(input?.StakeHolder))
            {
                instancesQuery = instancesQuery.Where(x => x.W2task.Email.ToString().Contains(input.StakeHolder));
            }

            var totalCount = instancesQuery.Count();
            var totalResults = await AsyncExecuter.ToListAsync(
                instancesQuery
                .OrderBy(NormalizeSortingString(input.Sorting))
                .Skip(input.SkipCount).Take(input.MaxResultCount)
                .Select(x => new
                {
                    instance = x.WorkflowInstance,
                    task = x.W2task
                })
            );
            
            var result = new List<WorkflowInstanceDto>();

            foreach (var res in totalResults)
            {
                var instance = res.instance;
                var task = res.task;

                var workflowDefinition = workflowDefinitions.FirstOrDefault(x => x.DefinitionId == instance.DefinitionId);
                var workflowInstanceDto = ObjectMapper.Map<WorkflowInstance, WorkflowInstanceDto>(instance);
                workflowInstanceDto.WorkflowDefinitionDisplayName = workflowDefinition.DisplayName;
                workflowInstanceDto.StakeHolders = new List<string>();
                workflowInstanceDto.CurrentStates = new List<string>();

                if (instance.WorkflowStatus == WorkflowStatus.Finished)
                {
                    var lastExecutedActivity = workflowDefinition.Activities.FirstOrDefault(x => x.ActivityId == instance.LastExecutedActivityId);
                    workflowInstanceDto.Status = GetFinalStatus(lastExecutedActivity);
                }

                string stakeHolderName = string.Empty;
                if (task!= null && task.Email != null)
                {
                    switch (task.Email)
                    {
                        case "it@ncc.asia":
                            stakeHolderName = "IT Department";
                            break;
                        case "sale@ncc.asia":
                            stakeHolderName = "Sale Department";
                            break;
                        default:
                            var stakeHolder = await _userRepository.FindByNormalizedEmailAsync(task.Email.ToUpper());
                            stakeHolderName = stakeHolder.Name;
                            break;
                    }

                    workflowInstanceDto.CurrentStates.Add(task.Description);
                    var requestUser = await _userRepository.FindAsync(task.Author);
                    workflowInstanceDto.UserRequestName = requestUser.Name;
                }

                workflowInstanceDto.StakeHolders.Add(stakeHolderName);
                result.Add(workflowInstanceDto);
            }

            return new PagedResultDto<WorkflowInstanceDto>(totalCount, result);
        }

        private string GetFinalStatus(ActivityDefinition activityDefinition)
        {
            return activityDefinition == null ? "Finished" : (activityDefinition.DisplayName.ToLower().Contains("reject") ? "Rejected" : "Approved");
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

        private string NormalizeSortingStringWFH(string sorting)
        {
            if (sorting.IsNullOrEmpty())
            {
                return "Email DESC";
            }

            var sortingPart = sorting.Trim().Split(' ');
            var direction = sortingPart[1].ToLower();
            if (direction == "asc")
            {
                return "Email ASC";
            }
            return "Email DESC";
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
                 new ListAllWorkflowDefinitionsSpecification(CurrentTenantStrId, new string[] { workflowInstance.DefinitionId }))).FirstOrDefault();

            var tasks = (await _taskRepository.FirstOrDefaultAsync(x => x.WorkflowInstanceId == workflowInstance.Id));
            var requestTasks = ObjectMapper.Map<W2Task, W2TasksDto>(tasks);

            var workflowInstanceDetailDto = new WorkflowInstanceDetailDto
            {
                workInstanceId = id,
                tasks = requestTasks,
                input = workflowInstance.Variables.Data,
                typeRequest = workflowDefinitions.DisplayName,
            };

            return workflowInstanceDetailDto;
        }
    }
}
