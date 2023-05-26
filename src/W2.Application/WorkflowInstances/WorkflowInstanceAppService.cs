using Elsa.Models;
using Elsa.Persistence;
using Elsa.Persistence.Specifications;
using Elsa.Persistence.Specifications.WorkflowInstances;
using Elsa.Services;
using Elsa.Services.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Emailing;
using Volo.Abp.TextTemplating;
using W2.Permissions;
using W2.Specifications;
using W2.Templates;

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

        public WorkflowInstanceAppService(IWorkflowLaunchpad workflowLaunchpad,
            IRepository<WorkflowInstanceStarter, Guid> instanceStarterRepository,
            IWorkflowInstanceStore workflowInstanceStore,
            IWorkflowDefinitionStore workflowDefinitionStore,
            IWorkflowInstanceCanceller canceller,
            IWorkflowInstanceDeleter workflowInstanceDeleter,
            IEmailSender emailSender,
            ITemplateRenderer templateRenderer,
            ILogger<WorkflowInstanceAppService> logger)
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
            var workflowInstanceStarter = new WorkflowInstanceStarter
            {
                WorkflowInstanceId = instance.Id,
                Input = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(input.Input))
            };

            await _instanceStarterRepository.InsertAsync(workflowInstanceStarter);

            CurrentUnitOfWork.OnCompleted(() =>
            {
                _logger.LogDebug("UOW CreateNewInstanceAsync completed.");
                return Task.CompletedTask;
            });

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
            _logger.LogDebug("Fetch WorkflowInstanceDto begin");
            var workflowInstanceStarter = await _instanceStarterRepository.FirstOrDefaultAsync(x => x.WorkflowInstanceId == id);
            if (workflowInstanceStarter != null)
            {
                instanceDto.CreatorId = workflowInstanceStarter.CreatorId;
            }
            else
            {
                _logger.LogError("Workflow not found");
            }

            return instanceDto;
        }

        public async Task<PagedResultDto<WorkflowInstanceDto>> ListAsync(ListAllWorkflowInstanceInput input)
        {
            var specification = Specification<WorkflowInstance>.Identity;
            if (CurrentTenant.IsAvailable)
            {
                specification = specification.WithTenant(CurrentTenantStrId);
            }
            if (!string.IsNullOrWhiteSpace(input?.WorkflowDefinitionId))
            {
                specification = specification.WithWorkflowDefinition(input.WorkflowDefinitionId);
            }
            var orderBySpecification = OrderBySpecification.OrderByDescending<WorkflowInstance>(x => x.CreatedAt!);

            var instances = (await _workflowInstanceStore.FindManyAsync(specification, orderBySpecification)).ToList();
            var instanceDtos = ObjectMapper.Map<List<WorkflowInstance>, List<WorkflowInstanceDto>>(instances);
            var workflowDefinitions = (await _workflowDefinitionStore.FindManyAsync(
                new ListAllWorkflowDefinitionsSpecification(CurrentTenantStrId, instances.Select(i => i.DefinitionId).ToArray())
            )).ToList();

            if (!await AuthorizationService.IsGrantedAsync(W2Permissions.WorkflowManagementWorkflowInstancesViewAll))
            {
                var workflowInstanceStarterIdsQuery = (await _instanceStarterRepository.GetQueryableAsync())
                    .Where(x => x.CreatorId == CurrentUser.Id)
                    .Select(x => x.WorkflowInstanceId);

                var workflowInstanceIds = await AsyncExecuter.ToListAsync(workflowInstanceStarterIdsQuery);

                instanceDtos = instanceDtos
                    .Where(x => workflowInstanceIds.Contains(x.Id))
                    .ToList();
            }

            foreach (var instanceDto in instanceDtos)
            {
                var workflowDefinition = workflowDefinitions.FirstOrDefault(x => x.DefinitionId == instanceDto.WorkflowDefinitionId);
                if (workflowDefinition == null)
                {
                    continue;
                }
                instanceDto.WorkflowDefinitionDisplayName = workflowDefinition.DisplayName;

            }

            return new PagedResultDto<WorkflowInstanceDto>(instanceDtos.Count, instanceDtos);
        }
    }
}
