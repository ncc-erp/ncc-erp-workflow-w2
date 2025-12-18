using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using W2.WorkflowDefinitions;
using W2.WorkflowInstances;
using Elsa.Persistence;
using Elsa.Persistence.Specifications.WorkflowDefinitions;
using Microsoft.Extensions.Logging;
using W2.Specifications;
using W2.Utils;

namespace W2.Activities
{
    public class RequestHistoryManager : DomainService
    {
        private readonly IRepository<W2RequestHistory, Guid> _requestHistoryRepository;
        private readonly IWorkflowDefinitionStore _workflowDefinitionStore;
        private readonly IRepository<WorkflowCustomInputDefinition, Guid> _workflowCustomInputDefinitionRepository;
        private readonly ILogger<RequestHistoryManager> _logger;

        public RequestHistoryManager(
            IRepository<W2RequestHistory, Guid> requestHistoryRepository,
            IWorkflowDefinitionStore workflowDefinitionStore,
            IRepository<WorkflowCustomInputDefinition, Guid> workflowCustomInputDefinitionRepository,
            ILogger<RequestHistoryManager> logger)
        {
            _requestHistoryRepository = requestHistoryRepository;
            _workflowDefinitionStore = workflowDefinitionStore;
            _workflowCustomInputDefinitionRepository = workflowCustomInputDefinitionRepository;
            _logger = logger;
        }

        public async Task CreateHistoryRecordsAsync(
            WorkflowInstanceStarter starter,
            string email)
        {
            // Get request type from workflow definition
            var requestType = await GetRequestTypeAsync(starter.WorkflowDefinitionId);

            // Extract dates from input based on property definitions
            var dates = await ExtractDatesFromInputAsync(starter.WorkflowDefinitionId, starter.Input);

            if (!dates.Any())
            {
                // If no specific dates, use creation time
                dates.Add(starter.CreationTime);
            }

            // Create history records for each date
            foreach (var date in dates)
            {
                var history = new W2RequestHistory
                {
                    WorkflowInstanceId = starter.WorkflowInstanceId,
                    WorkflowDefinitionId = starter.WorkflowDefinitionId,
                    WorkflowInstanceStarterId = starter.Id,
                    Email = email,
                    Date = date,
                    Status = starter.Status,
                    RequestType = requestType,
                    TenantId = starter.TenantId
                };

                await _requestHistoryRepository.InsertAsync(history, autoSave: true);
            }
        }

        public async Task UpdateHistoryStatusAsync(
            Guid workflowInstanceStarterId,
            WorkflowInstancesStatus newStatus)
        {
            var histories = await _requestHistoryRepository.GetListAsync(
                x => x.WorkflowInstanceStarterId == workflowInstanceStarterId);

            foreach (var history in histories)
            {
                history.Status = newStatus;
                await _requestHistoryRepository.UpdateAsync(history, autoSave: true);
            }
        }

        private async Task<string> GetRequestTypeAsync(string workflowDefinitionId)
        {
            var tenantId = CurrentTenant?.Id?.ToString();

            var specification = new ListAllWorkflowDefinitionsSpecification(tenantId, new[] { workflowDefinitionId });
            var definition = await _workflowDefinitionStore.FindAsync(specification);

            if (definition == null)
            {
                definition = await _workflowDefinitionStore.FindAsync(new WorkflowDefinitionIdSpecification(workflowDefinitionId));
            }

            if (!string.IsNullOrWhiteSpace(definition.Tag))
            {
                return definition.Tag;
            }
            return "Default";
        }

        private async Task<List<DateTime>> ExtractDatesFromInputAsync(string workflowDefinitionId, Dictionary<string, string> input)
        {
            var result = new HashSet<DateTime>();

            if (input == null || input.Count == 0)
            {
                return result.ToList();
            }

            var definition = await _workflowCustomInputDefinitionRepository
                .FirstOrDefaultAsync(x => x.WorkflowDefinitionId == workflowDefinitionId);

            if (definition?.PropertyDefinitions == null || !definition.PropertyDefinitions.Any())
            {
                return result.ToList();
            }

            var dateProperties = definition.PropertyDefinitions
                .Where(p => DateTimeHelper.IsDateType(p.Type))
                .ToList();

            if (!dateProperties.Any())
            {
                return result.ToList();
            }

            foreach (var property in dateProperties)
            {
                if (!input.TryGetValue(property.Name, out var raw) || string.IsNullOrWhiteSpace(raw))
                {
                    continue;
                }

                var parsedDates = DateTimeHelper.ParseDateValues(raw);
                foreach (var date in parsedDates)
                {
                    result.Add(date.Date);
                }
            }

            return result.OrderBy(d => d).ToList();
        }
    }
}
