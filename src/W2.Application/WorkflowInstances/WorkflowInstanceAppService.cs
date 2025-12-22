using Elsa;
using Elsa.Models;
using Elsa.Persistence;
using Elsa.Persistence.Specifications;
using Elsa.Persistence.Specifications.WorkflowInstances;
using Elsa.Services;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NodaTime;
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
using Volo.Abp.Identity;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Uow;
using Volo.Abp.Users;
using W2.Authorization.Attributes;
using W2.Constants;
using W2.ExternalResources;
using W2.Identity;
using W2.Komu;
using W2.Permissions;
using W2.Specifications;
using W2.TaskActions;
using W2.Tasks;
using W2.Utils;
using W2.Webhooks;
using W2.WorkflowDefinitions;

namespace W2.WorkflowInstances
{
    //[Authorize(W2Permissions.WorkflowManagementWorkflowInstances)]
    [RequirePermission(W2ApiPermissions.WorkflowInstancesManagement)]
    public class WorkflowInstanceAppService : W2AppService, IWorkflowInstanceAppService
    {
        private readonly IWorkflowLaunchpad _workflowLaunchpad;
        private readonly IRepository<WorkflowInstanceStarter, Guid> _instanceStarterRepository;
        private readonly IRepository<W2Task, Guid> _taskRepository;
        private readonly IRepository<W2TaskActions, Guid> _taskActionsRepository;
        private readonly IRepository<WFHHistory, Guid> _wfhHistoryRepository;
        private readonly IRepository<W2RequestHistory, Guid> _requestHistoryRepository;
        private readonly IRepository<W2RequestHistory, Guid> _w2RequestHistoryRepository;
        private readonly ILocalEventBus _localEventBus;
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
        private readonly IKomuAppService _komuAppService;
        private readonly IWebhookSender _webhookSender;
        public WorkflowInstanceAppService(IWorkflowLaunchpad workflowLaunchpad,
            IRepository<WorkflowInstanceStarter, Guid> instanceStarterRepository,
            IRepository<W2Task, Guid> taskRepository,
                        IRepository<W2TaskActions, Guid> taskActionsRepository,
                        IRepository<WFHHistory, Guid> wfhHistoryRepository,
            IRepository<W2RequestHistory, Guid> w2RequestHistoryRepository,
            ILocalEventBus localEventBus,
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
            IExternalResourceAppService externalResourceAppService,
            IKomuAppService komuAppService,
            IWebhookSender webhookSender
            )
        {
            _workflowLaunchpad = workflowLaunchpad;
            _instanceStarterRepository = instanceStarterRepository;
            _w2RequestHistoryRepository = w2RequestHistoryRepository;
            _localEventBus = localEventBus;
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
            _wfhHistoryRepository = wfhHistoryRepository;
            _dataFilter = dataFilter;
            _currentUser = currentUser;
            _workflowCustomInputDefinitionRepository = workflowCustomInputDefinitionRepository;
            _externalResourceAppService = externalResourceAppService;
            _komuAppService = komuAppService;
            _webhookSender = webhookSender;
        }

        [RequirePermission(W2ApiPermissions.CancelWorkflowInstance)]
        public async Task<string> CancelAsync(string id)
        {
            var workflowInstance = await _workflowInstanceStore.FindByIdAsync(id);
            var isAdmin = _currentUser.IsInRole("admin");
            var currentUserId = _currentUser.Id;

            var workflowInstanceStarter = await _instanceStarterRepository.FirstOrDefaultAsync(x => x.WorkflowInstanceId == id);
            if (workflowInstanceStarter == null)
            {
                throw new UserFriendlyException(L["Cancel Failed: workflowInstanceStarter Is Not Found!"]);
            }

            else if (!isAdmin && workflowInstanceStarter.Status != WorkflowInstancesStatus.Pending)
            {
                throw new UserFriendlyException(L[" Cannot cancel a request if status is not Pending!"]);
            }
            var isMyRequest = workflowInstanceStarter.CreatorId == currentUserId;


            if (!(isAdmin || isMyRequest))
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
            
            // Emit event to update history status
            await _localEventBus.PublishAsync(new RequestHistoryStatusChangedEvent
            {
                WorkflowInstanceStarterId = workflowInstanceStarter.Id,
                NewStatus = WorkflowInstancesStatus.Canceled
            });

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

        //[Authorize(W2Permissions.WorkflowManagementWorkflowInstancesCreate)]
        [RequirePermission(W2ApiPermissions.CreateWorkflowInstance)]
        public async Task<object> CreateNewInstanceAsync(CreateNewWorkflowInstanceDto input)
        {
            var startableWorkflow = await _workflowLaunchpad.FindStartableWorkflowAsync(input.WorkflowDefinitionId, tenantId: CurrentTenantStrId);

            if (startableWorkflow == null)
            {
                throw new UserFriendlyException(L["Exception:NoStartableWorkflowFound"]);
            }

            var httpRequestModel = GetHttpRequestModel(nameof(HttpMethod.Post), input.Input);

            var executionResult = await _workflowLaunchpad.ExecuteStartableWorkflowAsync(startableWorkflow, new WorkflowInput(httpRequestModel));

            var instance = executionResult.WorkflowInstance;
            var workflowInstanceStarterResponse = new WorkflowInstanceStarter();
            using (var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false))
            {
                var workflowInstanceStarter = new WorkflowInstanceStarter
                {
                    WorkflowInstanceId = instance.Id,
                    WorkflowDefinitionId = instance.DefinitionId,
                    WorkflowDefinitionVersionId = instance.DefinitionVersionId,
                    Input = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(input.Input))
                };

                workflowInstanceStarterResponse = await _instanceStarterRepository.InsertAsync(workflowInstanceStarter);

                var currentUserEmail = CurrentUser.Email ?? CurrentUser.UserName;
                if (!string.IsNullOrEmpty(currentUserEmail))
                {
                    // Emit event to create history records
                    await _localEventBus.PublishAsync(new RequestHistoryCreatedEvent
                    {
                        Starter = workflowInstanceStarterResponse,
                        Email = currentUserEmail
                    });
                }

                await uow.CompleteAsync();
                await _webhookSender.SendCreatedRequest("Request Created", new
                {
                    WorkflowInstanceId = instance.Id,
                    WorkflowDefinitionId = instance.DefinitionId,
                    CreatorId = workflowInstanceStarterResponse.CreatorId,
                });

                _logger.LogInformation("Saved changes to database");
            }


            return workflowInstanceStarterResponse;
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

        [AllowAnonymous]
        public async Task<object> GetWfhCount(
                        [RegularExpression("^\\d{4}\\-(0[1-9]|1[012])\\-(0[1-9]|[12][0-9]|3[01])$", ErrorMessage = "Invalid Date yyyy-MM-dd")]
            string from,
            [RegularExpression("^\\d{4}\\-(0[1-9]|1[012])\\-(0[1-9]|[12][0-9]|3[01])$", ErrorMessage = "Invalid Date yyyy-MM-dd")]
            string to,
            [Required]
            Int16 limit,
            [Required]
            Int32 offset,
            WorkflowInstancesStatus status = WorkflowInstancesStatus.Approved)
        {// date "dd/MM/yyyy"
            // fake todo change
            if (from.IsNullOrEmpty())
            {
                from = "2000-01-01";
            }
            if (to.IsNullOrEmpty())
            {
                to = "3000-01-01";
            }
            var dateFromArray = from.Split("-");
            var dateFromDb = Int64.Parse($"{dateFromArray[0]}{dateFromArray[1]}{dateFromArray[2]}");
            var dateToArray = to.Split("-");
            var dateToDb = Int64.Parse($"{dateToArray[0]}{dateToArray[1]}{dateToArray[2]}");
            string defaultWFHDefinitionsId = _configuration.GetValue<string>("DefaultWFHDefinitionsId");

            var wfhQuery = await _wfhHistoryRepository.GetQueryableAsync();
            var latestWfhHistory = wfhQuery.OrderByDescending(x => x.CreationTime).FirstOrDefault();
            // migrate before get
            var wfInstanceQuery = await _instanceStarterRepository.GetQueryableAsync();
            wfInstanceQuery = wfInstanceQuery.Where(x => x.WorkflowDefinitionId == defaultWFHDefinitionsId);
            //wfInstanceQuery.Join(XmlConfig)
            if (latestWfhHistory != null)
            {
                wfInstanceQuery = wfInstanceQuery.Where(w => w.CreationTime > latestWfhHistory.CreationTime);
            }
            // migrate all item
            var listUsers = wfInstanceQuery.Select(w => w.CreatorId).ToList();
            var userMap = (await _userRepository.GetListAsync()).Where(u => listUsers.Contains(u.Id)).ToDictionary(x => x.Id.ToString(), x => x.Email);
            foreach (var wfh in wfInstanceQuery.ToList())
            {
                // migrate
                try
                {
                    var remoteDates = wfh.Input.GetItem<string>("Dates").Split(",");// all remote dates, "Dates":"08/04/2024,09/04/2024"
                    foreach (var remoteDate in remoteDates)
                    {
                        var dateArray = remoteDate.Split("/");
                        var dateNumberDb = Int64.Parse($"{dateArray[2]}{dateArray[1]}{dateArray[0]}");
                        await _wfhHistoryRepository.InsertAsync(new WFHHistory
                        {
                            RemoteDate = dateNumberDb,
                            Branch = wfh.Input.GetItem<string>("CurrentOffice"),
                            Email = userMap.GetItem(wfh.CreatorId.ToString()),
                            RequestUser = wfh.CreatorId,
                            WorkflowDefinitionId = wfh.WorkflowDefinitionId,
                            WorkflowInstanceId = wfh.WorkflowInstanceId,
                            WorkflowInstanceStarterId = wfh.Id,
                        }, true);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Migrate wfh error: " + wfh, ex);
                }
            }

            //

            var wfInstanceQueryJoin = (await _instanceStarterRepository.GetQueryableAsync()).Where(x => x.WorkflowDefinitionId == defaultWFHDefinitionsId);

            var totalCount = wfhQuery.Where(w => w.RemoteDate >= dateFromDb && w.RemoteDate <= dateToDb)
                .Join(wfInstanceQueryJoin, // the source table of the inner join
                  x => x.WorkflowInstanceStarterId,        // Select the primary key (the first part of the "on" clause in an sql "join" statement)
                  y => y.Id,   // Select the foreign key (the second part of the "on" clause)
                  (h, wf) => new { history = h, wf }) // selection
               .Where(wf => wf.wf.Status == status)
                .GroupBy(g => g.history.Email).Count();
            var result = from w in wfhQuery
                         where w.RemoteDate >= dateFromDb && w.RemoteDate <= dateToDb
                         join wf in wfInstanceQueryJoin on w.WorkflowInstanceStarterId equals wf.Id
                         where wf.Status == status
                         group w by new { w.Email, w.Branch } into egb
                         select new
                         {
                             egb.Key.Email,
                             egb.Key.Branch,
                             totalRemoteDay = egb.Select(e => e.RemoteDate).Distinct().Count(),
                             Dates = egb.Select(e => e.RemoteDate).Distinct().ToList(),
                             totalRemoteCount = wfhQuery.Where(w => w.RemoteDate >= dateFromDb && w.RemoteDate <= dateToDb)
                                .Where(w => w.Email == egb.Key.Email)
                                .Join(wfInstanceQueryJoin, // the source table of the inner join
                                  x => x.WorkflowInstanceStarterId,        // Select the primary key (the first part of the "on" clause in an sql "join" statement)
                                  y => y.Id,   // Select the foreign key (the second part of the "on" clause)
                                  (h, wf) => new { history = h, wf }) // selection
                                .Where(wf => wf.wf.Status == status)
                                .GroupBy(g => g.history.WorkflowInstanceId).Count(),
                         };
            return new
            {
                totalCount,
                data = result.Skip(offset).Take(limit).ToList(),
            };
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
            }
            ;

            return result;
        }

        [RequirePermission(W2ApiPermissions.ViewListWFHReport)]
        public async Task<PagedResultDto<WFHRequestDto>> GetWfhListAsync(ListAllWFHRequestInput input)
        {
            string defaultWFHDefinitionsId = _configuration.GetValue<string>("DefaultWFHDefinitionsId");
            var wfhCountObject = await GetWfhCount("2000-01-01", "3000-01-01", 100, 0);

            var wfhQuery = await _wfhHistoryRepository.GetQueryableAsync();
            var wfInstanceQueryJoin = (await _instanceStarterRepository.GetQueryableAsync()).Where(x => x.WorkflowDefinitionId == defaultWFHDefinitionsId);

            var dateFromArray = input.StartDate.IsNullOrEmpty() ? "2000-01-01".Split("-") : input.StartDate.Split("-");
            var dateFromDb = Int64.Parse($"{dateFromArray[0]}{dateFromArray[1]}{dateFromArray[2]}");
            var dateToArray = input.EndDate.IsNullOrEmpty() ? "3000-01-01".Split("-") : input.EndDate.Split("-");
            var dateToDb = Int64.Parse($"{dateToArray[0]}{dateToArray[1]}{dateToArray[2]}");

            var totalCountQuery = wfhQuery.Where(w => w.RemoteDate >= dateFromDb && w.RemoteDate <= dateToDb)
            .Join(wfInstanceQueryJoin,
              x => x.WorkflowInstanceStarterId,
              y => y.Id,
              (h, wf) => new { history = h, wf })
            .Where(wf => input.Status == WorkflowInstancesStatus.All || wf.wf.Status == input.Status);

            var joinQuery = (from w in wfhQuery
                             where w.RemoteDate >= dateFromDb && w.RemoteDate <= dateToDb
                             join wf in wfInstanceQueryJoin on w.WorkflowInstanceStarterId equals wf.Id
                             let status = wf.Status
                             where input.Status == WorkflowInstancesStatus.All || status == input.Status
                             select new WFHRequestDto
                             {
                                 Id = w.Id,
                                 Branch = w.Branch,
                                 RemoteDate = w.RemoteDate,
                                 Email = w.Email,
                                 CreationTime = w.CreationTime,
                                 Reason = wf.Input["Reason"].ToString(),
                                 Status = wf.Status
                             });

            if (!string.IsNullOrEmpty(input.KeySearch))
            {
                totalCountQuery = totalCountQuery.Where(wf => wf.history.Email.Contains(input.KeySearch));
                joinQuery = joinQuery.Where(w => w.Email.Contains(input.KeySearch));
            }

            var totalCount = totalCountQuery.Count();

            var result = ApplySorting(joinQuery, input.Sorting).Skip(input.SkipCount).Take(input.MaxResultCount).ToList();

            return new PagedResultDto<WFHRequestDto>(totalCount, result);
        }

        [RequirePermission(W2ApiPermissions.ViewListWorkflowInstances)]
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
                switch (input.Status)
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

            var listDefineIds = totalResults.Select(x => x.instanceStarter.WorkflowDefinitionId).ToList();
            var inputDefinitions = await _workflowCustomInputDefinitionRepository
            .GetListAsync(x => listDefineIds.Contains(x.WorkflowDefinitionId));

            var allDefines = (await _workflowCustomInputDefinitionRepository.GetQueryableAsync())
                .Where(i => listDefineIds.Contains(i.WorkflowDefinitionId))
                .ToDictionary(x => x.WorkflowDefinitionId, x => x);

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
                workflowInstanceDto.Settings = new SettingsDto { Color = "#aabbcc", TitleTemplate = "" };
                workflowInstanceDto.Settings.Color = inputDefinitions.FirstOrDefault(i => i.WorkflowDefinitionId == workflowInstanceDto.WorkflowDefinitionId)?.Settings?.Color ?? "#aabbcc";
                workflowInstanceDto.Settings.TitleTemplate = inputDefinitions.FirstOrDefault(i => i.WorkflowDefinitionId == workflowInstanceDto.WorkflowDefinitionId)?.Settings?.TitleTemplate ?? "";
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
                if (workflowDefinition != null && allDefines.ContainsKey(workflowDefinition.DefinitionId))
                {
                    var titleFiled = allDefines.GetItem(workflowDefinition.DefinitionId);


                    var InputClone = new Dictionary<string, string>(workflowInstanceStarter.Input)
                    {
                        { "RequestUser", workflowInstanceDto.UserRequestName }
                    };
                    var title = TitleTemplateParser.ParseTitleTemplateToString(titleFiled.Settings?.TitleTemplate ?? "", InputClone);
                    workflowInstanceDto.ShortTitle = title;
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

        private IQueryable<WFHRequestDto> ApplySorting(IQueryable<WFHRequestDto> query, string sorting)
        {
            if (string.IsNullOrEmpty(sorting))
            {
                return query.OrderBy(u => u.CreationTime);
            }

            var sortingParts = sorting.Trim().Split(' ');
            if (sortingParts.Length != 2)
            {
                return query.OrderBy(u => u.CreationTime);
            }

            var property = sortingParts[0].ToLower();
            var isAscending = sortingParts[1].ToLower() == "asc";

            query = property switch
            {
                "email" => isAscending
                    ? query.OrderBy(u => u.Email)
                    : query.OrderByDescending(u => u.Email),

                "reason" => isAscending
                    ? query.OrderBy(u => u.Reason)
                    : query.OrderByDescending(u => u.Reason),

                "remotedate" => isAscending
                    ? query.OrderBy(u => u.RemoteDate)
                    : query.OrderByDescending(u => u.RemoteDate),

                "creationtime" => isAscending
                    ? query.OrderBy(u => u.CreationTime)
                    : query.OrderByDescending(u => u.CreationTime),

                _ => query.OrderBy(u => u.CreationTime)
            };

            return query;
        }

        [RequirePermission(W2ApiPermissions.ViewListWorkflowInstances)]
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

        [RequirePermission(W2ApiPermissions.ViewListWorkflowInstances)]
        public async Task<List<RequestStatusDto>> GetRequestStatusAsync(GetRequestStatusInput input)
        {
            var query = await _w2RequestHistoryRepository.GetQueryableAsync();
            
            if (!string.IsNullOrWhiteSpace(input.Email))
            {
                query = query.Where(x => x.Email == input.Email);
            }

            if (input.Date.HasValue)
            {
                var dateOnly = input.Date.Value.Date;
                query = query.Where(x => x.Date.Date == dateOnly);
            }

            var histories = await AsyncExecuter.ToListAsync(
                query.OrderByDescending(x => x.CreationTime)
            );

            return histories.Select(h => new RequestStatusDto
            {
                Email = h.Email,
                Date = h.Date,
                Status = h.Status,
                Type = h.RequestType,
            }).ToList();
        }
    }
}
