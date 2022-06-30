using Elsa.Models;
using Elsa.Persistence;
using Elsa.Persistence.Specifications;
using Elsa.Services;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using W2.Permissions;
using W2.Specifications;

namespace W2.WorkflowDefinitions
{
    [Authorize(W2Permissions.WorkflowManagementWorkflowDefinitions)]
    public class WorkflowDefinitionAppService : W2AppService, IWorkflowDefinitionAppService
    {
        private readonly IWorkflowDefinitionStore _workflowDefinitionStore;
        private readonly IWorkflowLaunchpad _workflowLaunchpad;
        private readonly IRepository<WorkflowCustomInputDefinition, Guid> _workflowCustomInputDefinitionRepository;

        public WorkflowDefinitionAppService(
            IWorkflowDefinitionStore workflowDefinitionStore, 
            IWorkflowLaunchpad workflowLaunchpad, 
            IRepository<WorkflowCustomInputDefinition, Guid> workflowCustomInputDefinitionRepository)
        {
            _workflowDefinitionStore = workflowDefinitionStore;
            _workflowLaunchpad = workflowLaunchpad;
            _workflowCustomInputDefinitionRepository = workflowCustomInputDefinitionRepository;
        }

        public async Task<ListResultDto<WorkflowDefinitionSummaryDto>> ListAllAsync()
        {
            var specification = new ManyWorkflowDefinitionsLatestVersionSpecification(CurrentTenantStrId, null);
            var workflowDefinitions = (await _workflowDefinitionStore
                .FindManyAsync(
                    specification,
                    new OrderBy<WorkflowDefinition>(x => x.Name!, SortDirection.Ascending)
                ))
                .ToList();
            var definitionIds = workflowDefinitions.Select(x => x.DefinitionId).ToList();
            var inputDefinitions = await _workflowCustomInputDefinitionRepository
                .GetListAsync(x => definitionIds.Contains(x.WorkflowDefinitionId));
            var workflowDefinitionSummaries = ObjectMapper.Map<List<WorkflowDefinition>, List<WorkflowDefinitionSummaryDto>>(workflowDefinitions);
            
            foreach (var summary in workflowDefinitionSummaries)
            {
                var inputDefinition = inputDefinitions.FirstOrDefault(x => x.WorkflowDefinitionId == summary.DefinitionId);
                if (inputDefinition == null)
                {
                    continue;
                }
                summary.InputDefinition = ObjectMapper.Map<WorkflowCustomInputDefinition, WorkflowCustomInputDefinitionDto>(inputDefinition);
            }

            return new ListResultDto<WorkflowDefinitionSummaryDto>(workflowDefinitionSummaries);
        }

        public async Task<string> CreateNewInstanceAsync(ExecuteWorkflowDefinitionDto input)
        {
            var startableWorkflow = await _workflowLaunchpad.FindStartableWorkflowAsync(input.WorkflowDefinitionId, tenantId: CurrentTenantStrId);
            
            if (startableWorkflow == null)
            {
                throw new UserFriendlyException(L["Exception:NoStartableWorkflowFound"]);
            }

            var httpRequestModel = new Elsa.Activities.Http.Models.HttpRequestModel(null, "POST", null, null, null, RawBody: JsonConvert.SerializeObject(input.Input, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() { NamingStrategy = new CamelCaseNamingStrategy(false, false) } }));

            var executionResult = await _workflowLaunchpad.ExecuteStartableWorkflowAsync(startableWorkflow, new WorkflowInput(httpRequestModel));

            return executionResult.WorkflowInstance.Id;
        }

        public async Task<WorkflowDefinitionSummaryDto> GetByDefinitionIdAsync(string definitionId)
        {
            var specification = new ManyWorkflowDefinitionsLatestVersionSpecification(CurrentTenantStrId, new string[] { definitionId });
            var workflowDefinition = await _workflowDefinitionStore.FindAsync(specification);
            if (workflowDefinition == null)
            {
                throw new UserFriendlyException(L["Exception:WorkflowDefinitionNotFound"]);
            }

            return ObjectMapper.Map<WorkflowDefinition, WorkflowDefinitionSummaryDto>(workflowDefinition);
        }

        public async Task CreateWorkflowInputDefinitionAsync(WorkflowCustomInputDefinitionDto input)
        {
            await _workflowCustomInputDefinitionRepository.InsertAsync(
                ObjectMapper.Map<WorkflowCustomInputDefinitionDto, WorkflowCustomInputDefinition>(input)
            );
        }
    }
}
