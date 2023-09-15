using Elsa.Models;
using Elsa.Persistence;
using Elsa.Persistence.Specifications;
using Elsa.Persistence.Specifications.WorkflowInstances;
using Elsa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rebus.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
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

namespace W2.WorkflowInstances
{
    [Authorize(W2Permissions.WorkflowManagementWorkflowInstances)]
    public class WorkflowInstanceAppService : W2AppService, IWorkflowInstanceAppService
    {
        private readonly IWorkflowLaunchpad _workflowLaunchpad;
        private readonly IRepository<WorkflowInstanceStarter, Guid> _instanceStarterRepository;
        private readonly IWorkflowInstanceStore _workflowInstanceStore;
        private readonly IWorkflowDefinitionStore _workflowDefinitionStore;
        private readonly IWorkflowInstanceCanceller _canceller;
        private readonly IWorkflowInstanceDeleter _workflowInstanceDeleter;
        private readonly IEmailSender _emailSender;
        private readonly ITemplateRenderer _templateRenderer;
        private readonly ILogger<WorkflowInstanceAppService> _logger;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IIdentityUserRepository _userRepository;
        private readonly IAntClientApi _antClientApi;
        private readonly IConfiguration _configuration;
        public WorkflowInstanceAppService(IWorkflowLaunchpad workflowLaunchpad,
            IRepository<WorkflowInstanceStarter, Guid> instanceStarterRepository,
            IWorkflowInstanceStore workflowInstanceStore,
            IWorkflowDefinitionStore workflowDefinitionStore,
            IWorkflowInstanceCanceller canceller,
            IWorkflowInstanceDeleter workflowInstanceDeleter,
            IEmailSender emailSender,
            ITemplateRenderer templateRenderer,
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
            _emailSender = emailSender;
            _templateRenderer = templateRenderer;
            _logger = logger;
            _unitOfWorkManager = unitOfWorkManager;
            _userRepository = userRepository;
            _antClientApi = antClientApi;
            _configuration = configuration;
        }

        public async Task CancelAsync(string id)
        {
            var cancelResult = await _canceller.CancelAsync(id);

            switch (cancelResult.Status)
            {
                case CancelWorkflowInstanceResultStatus.NotFound:
                    throw new UserFriendlyException(L["Exception:InstanceNotFound"]);
                case CancelWorkflowInstanceResultStatus.InvalidStatus:
                    throw new UserFriendlyException(L["Exception:CancelInstanceInvalidStatus", cancelResult.WorkflowInstance!.WorkflowStatus]);
            }
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

        public async Task DeleteAsync(string id)
        {
            var result = await _workflowInstanceDeleter.DeleteAsync(id);
            if (result.Status == DeleteWorkflowInstanceResultStatus.NotFound)
            {
                throw new UserFriendlyException(L["Exception:InstanceNotFound"]);
            }
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
        public async Task<WorkflowStatusDto> GetWfhStatusAsync([Required] string email, [Required] DateTime date)
        {
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

            var requestUser = (await _userRepository.GetListAsync())
                .FirstOrDefault(x => x.Email == email) ?? throw new UserFriendlyException(L["Exception:EmailNotFound"]);

            var allWorkflowInstanceStarters = await AsyncExecuter.ToListAsync(await _instanceStarterRepository.GetQueryableAsync());
            workflowInstanceStarters = allWorkflowInstanceStarters
                .Where(x => x != null && instancesIds.Contains(x.WorkflowInstanceId) && x.Input != null && x.Input.ContainsValue(date.ToUniversalTime().ToString("dd/MM/yyyy")) && x.CreatorId == requestUser.Id)
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
                var workflowInstanceStarter = workflowInstanceStarters.FirstOrDefault(x => x.WorkflowInstanceId == instance.Id);
                var workflowInstanceDto = ObjectMapper.Map<WorkflowInstance, WorkflowStatusDto>(instance);
                workflowInstanceDto.Email = email;
                workflowInstanceDto.Date = DateTime.ParseExact(workflowInstanceStarter.Input.GetValue("Dates"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                result = workflowInstanceDto;
            }
            if (result.Email == null)
            {
                var newInstanceError = new WorkflowStatusDto
                {
                    Email = email,
                    Date = date,
                    Status = "-1"
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
            if (!await AuthorizationService.IsGrantedAsync(W2Permissions.WorkflowManagementWorkflowInstancesViewAll))
            {
                workflowInstanceStarters = await AsyncExecuter.ToListAsync((await _instanceStarterRepository.GetQueryableAsync())
                                .Where(x => instancesIds.Contains(x.WorkflowInstanceId) && x.CreatorId == CurrentUser.Id));
            }
            else
            {
                workflowInstanceStarters = await AsyncExecuter.ToListAsync((await _instanceStarterRepository.GetQueryableAsync())
                                .Where(x => instancesIds.Contains(x.WorkflowInstanceId)));
            }

            var totalCount = workflowInstanceStarters.Count();
            instances = await AsyncExecuter.ToListAsync(
                workflowInstanceStarters.Join(instances, x => x.WorkflowInstanceId, x => x.Id, (WorkflowInstanceStarter, WorkflowInstance) => new
                {
                    WorkflowInstanceStarter,
                    WorkflowInstance
                })
                .AsQueryable().OrderBy(NormalizeSortingString(input.Sorting))
                .Skip(input.SkipCount).Take(input.MaxResultCount)
                .Select(x => x.WorkflowInstance)
            );

            var requestUserIds = workflowInstanceStarters.Select(x => (Guid)x.CreatorId);
            var requestUsers = (await _userRepository.GetListAsync())
                .Where(x => x.Id.IsIn(requestUserIds))
                .ToList();
            var result = new List<WorkflowInstanceDto>();
            var stakeHolderEmails = new Dictionary<string, string>();

            foreach (var instance in instances)
            {
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

                var workflowInstanceStarter = workflowInstanceStarters.FirstOrDefault(x => x.WorkflowInstanceId == instance.Id);
                if (workflowInstanceStarter is not null)
                {
                    var identityUser = requestUsers.FirstOrDefault(x => x.Id == workflowInstanceStarter.CreatorId.Value);

                    if (identityUser != null && !stakeHolderEmails.ContainsKey(identityUser.Email))
                    {
                        stakeHolderEmails.Add(identityUser.Email, identityUser.Name);
                    }

                    workflowInstanceDto.UserRequestName = stakeHolderEmails[identityUser.Email];
                }

                var blockingActivityIds = instance.BlockingActivities.Select(x => x.ActivityId);
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

                        if (!workflowInstanceDto.CurrentStates.Contains(parentActivity.DisplayName))
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

                        if (data != null && data.ContainsKey("To"))
                        {
                            foreach (var email in (List<string>)data["To"])
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
    }
}
