using Elsa;
using Elsa.Models;
using Elsa.Persistence;
using Elsa.Persistence.Specifications;
using Elsa.Services;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NodaTime.Serialization.JsonNet;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using W2.Identity;
using W2.Permissions;
using W2.Specifications;

namespace W2.WorkflowDefinitions
{
    [Authorize(W2Permissions.WorkflowManagementWorkflowDefinitions)]
    public class WorkflowDefinitionAppService : W2AppService, IWorkflowDefinitionAppService
    {
        private readonly IWorkflowDefinitionStore _workflowDefinitionStore;
        private readonly IRepository<WorkflowCustomInputDefinition, Guid> _workflowCustomInputDefinitionRepository;
        private readonly IWorkflowPublisher _workflowPublisher;

        public WorkflowDefinitionAppService(
            IWorkflowDefinitionStore workflowDefinitionStore,
            IRepository<WorkflowCustomInputDefinition, Guid> workflowCustomInputDefinitionRepository,
            IWorkflowPublisher workflowPublisher)
        {
            _workflowDefinitionStore = workflowDefinitionStore;
            _workflowCustomInputDefinitionRepository = workflowCustomInputDefinitionRepository;
            _workflowPublisher = workflowPublisher;
        }
        public async Task<PagedResultDto<WorkflowDefinitionSummaryDto>> ListAllAsync(bool? isPublish)
        {
            var specification = Specification<WorkflowDefinition>.Identity;
            specification = specification.And(new ListAllWorkflowDefinitionsSpecification(CurrentTenantStrId, null));

            if (!CurrentUser.IsInRole("admin") && !CurrentUser.IsInRole(RoleNames.Designer))
            {
                specification = specification.And(new PublishedWorkflowDefinitionsSpecification());
            }

            var workflowDefinitionsFound = (await _workflowDefinitionStore
                .FindManyAsync(
                    specification,
                    new OrderBy<WorkflowDefinition>(x => x.Name!, SortDirection.Ascending)
                ));
            List<WorkflowDefinition> workflowDefinitions;

            if (isPublish == true)
            {
                workflowDefinitions = workflowDefinitionsFound.Where(w => w.IsPublished).ToList();
            }
            else
            {
                workflowDefinitions = workflowDefinitionsFound
                                        .Where(w => w.IsLatest)
                                        .OrderByDescending(w => w.IsPublished)
                                        .ToList();
            }
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
                var workflowDefinition = workflowDefinitions.FirstOrDefault(el => el.DefinitionId == summary.DefinitionId);
                if (workflowDefinition == null)
                {
                    continue;
                }
                var jsonString = JsonConvert.SerializeObject(workflowDefinition, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
                summary.DefineJson = jsonString;

                summary.InputDefinition = ObjectMapper.Map<WorkflowCustomInputDefinition, WorkflowCustomInputDefinitionDto>(inputDefinition);
            }

            return new PagedResultDto<WorkflowDefinitionSummaryDto>(workflowDefinitionSummaries.Count, workflowDefinitionSummaries);
        }

        public async Task<WorkflowDefinitionSummaryDto> GetByDefinitionIdAsync(string definitionId)
        {
            var specification = new ListAllWorkflowDefinitionsSpecification(CurrentTenantStrId, new string[] { definitionId });
            var workflowDefinition = await _workflowDefinitionStore.FindAsync(specification);
            if (workflowDefinition == null)
            {
                throw new UserFriendlyException(L["Exception:WorkflowDefinitionNotFound"]);
            }

            var workflowDefinitionDto = ObjectMapper.Map<WorkflowDefinition, WorkflowDefinitionSummaryDto>(workflowDefinition);
            var inputDefinition = await _workflowCustomInputDefinitionRepository
                .FindAsync(x => x.WorkflowDefinitionId == workflowDefinition.DefinitionId);
            if (inputDefinition != null)
            {
                workflowDefinitionDto.InputDefinition = ObjectMapper.Map<WorkflowCustomInputDefinition, WorkflowCustomInputDefinitionDto>(inputDefinition);
            }

            return workflowDefinitionDto;
        }

        [Authorize(W2Permissions.WorkflowManagementWorkflowDefinitionsDesign)]
        public async Task SaveWorkflowInputDefinitionAsync(WorkflowCustomInputDefinitionDto input)
        {
            if (input.DefineJson != null) await UpdateWorkflowDefinitionAsync(input.DefineJson, input.WorkflowDefinitionId);

            if (input.Id == default)
            {
                await _workflowCustomInputDefinitionRepository.InsertAsync(
                    ObjectMapper.Map<WorkflowCustomInputDefinitionDto, WorkflowCustomInputDefinition>(input)
                );
            }
            else
            {
                var inputDefinition = await _workflowCustomInputDefinitionRepository.GetAsync(input.Id);
                ObjectMapper.Map(input, inputDefinition);
                await _workflowCustomInputDefinitionRepository.UpdateAsync(inputDefinition);
            }
        }

        private async Task UpdateWorkflowDefinitionAsync(string defineJson, string currentWorkflowDefineId)
        {
            var workflowDefinition = JsonConvert.DeserializeObject<WorkflowDefinition>(defineJson, new JsonSerializerSettings().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb));

            workflowDefinition.DefinitionId = currentWorkflowDefineId;

            var existingWorkflowDefinition = await _workflowDefinitionStore.FindByDefinitionIdAsync( workflowDefinition.DefinitionId, VersionOptions.Latest);

            if (existingWorkflowDefinition != null)
            {
                workflowDefinition.IsPublished = false;
                workflowDefinition.Version = existingWorkflowDefinition.Version + 1;
                workflowDefinition.Id = existingWorkflowDefinition.Id;
                await _workflowDefinitionStore.UpdateAsync(workflowDefinition);
            }
        }

        [Authorize(W2Permissions.WorkflowManagementWorkflowDefinitionsDesign)]
        public async Task<string> CreateWorkflowDefinitionAsync(CreateWorkflowDefinitionDto input)
        {
            var workflowDefinition = ObjectMapper.Map<CreateWorkflowDefinitionDto, WorkflowDefinition>(input);

            var workflowDraft = await _workflowPublisher.SaveDraftAsync(workflowDefinition);

            if (input.workflowCreateData != null)
            {
                var workflowCreateData = input.workflowCreateData;
                workflowCreateData.Id = default;
                workflowCreateData.WorkflowDefinitionId = workflowDraft.DefinitionId;
                await SaveWorkflowInputDefinitionAsync(workflowCreateData);
            };
            return workflowDraft.DefinitionId;
        }

        [Authorize(W2Permissions.WorkflowManagementWorkflowDefinitionsDesign)]
        public async Task DeleteAsync(string id)
        {
            await _workflowPublisher.DeleteAsync(id, VersionOptions.All);
        }
        public async Task<object> ChangeWorkflowStatusAsync(UpdateWorkflowPublishStatusDto input)
        {
            var specification = new ListAllWorkflowDefinitionsSpecification(CurrentTenantStrId, new string[] { input.WorkflowId });
            var workflowDefinition = await _workflowDefinitionStore.FindAsync(specification);

            if (workflowDefinition == null)
            {
                throw new UserFriendlyException(L["Exception:WorkflowDefinitionNotFound"]);
            }

            workflowDefinition.IsPublished = input.IsPublished;

            if (input.IsPublished)
            {
                await _workflowPublisher.PublishAsync(workflowDefinition);
            }
            else
            {
                await _workflowPublisher.SaveDraftAsync(workflowDefinition);
            }

            return new
            {
                WorkflowId = input.WorkflowId,
                IsPublished = input.IsPublished,
                Message = $"Workflow with ID {input.WorkflowId} updated to published: {input.IsPublished}"
            };
        }
    }
}
