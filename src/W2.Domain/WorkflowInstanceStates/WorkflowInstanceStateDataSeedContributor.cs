using Elsa.Models;
using Elsa.Persistence;
using Elsa.Persistence.Specifications;
using Open.Linq.AsyncExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using W2.Specifications;
using W2.WorkflowInstances;

namespace W2.WorkflowInstanceStates
{
    public class WorkflowInstanceStateDataSeedContributor : IDataSeedContributor, ITransientDependency
    {
        private readonly IRepository<WorkflowInstanceStarter, Guid> _instanceStarterRepository;
        private readonly IWorkflowInstanceStore _workflowInstanceStore;
        private readonly IWorkflowDefinitionStore _workflowDefinitionStore;
        private readonly WorkflowInstanceStarterManager _workflowInstanceStarterManager;

        public WorkflowInstanceStateDataSeedContributor(
            IRepository<WorkflowInstanceStarter, Guid> instanceStarterRepository, 
            IWorkflowInstanceStore workflowInstanceStore, 
            IWorkflowDefinitionStore workflowDefinitionStore, 
            WorkflowInstanceStarterManager workflowInstanceStarterManager)
        {
            _instanceStarterRepository = instanceStarterRepository;
            _workflowInstanceStore = workflowInstanceStore;
            _workflowDefinitionStore = workflowDefinitionStore;

            _workflowInstanceStarterManager = workflowInstanceStarterManager;
        }

        public async Task SeedAsync(DataSeedContext context)
        {
            var instances = await _workflowInstanceStore.FindManyAsync(Specification<WorkflowInstance>.Identity);
            var workflowDefinitions = await _workflowDefinitionStore.FindManyAsync(new ListAllWorkflowDefinitionsSpecification(string.Empty, instances.Select(x => x.DefinitionId).Distinct().ToArray()));

            var instancesIds = instances.Select(x => x.Id);
            var workflowInstanceStarters = await _instanceStarterRepository.GetListAsync(x => instancesIds.Contains(x.WorkflowInstanceId));

            var stakeHolderEmails = new Dictionary<string, IdentityUser>();
            foreach (var instance in instances)
            {
                var workflowDefinition = workflowDefinitions.FirstOrDefault(x => x.DefinitionId == instance.DefinitionId);
                var workflowInstanceStarter = workflowInstanceStarters.FirstOrDefault(x => x.WorkflowInstanceId == instance.Id);

                if (workflowInstanceStarter is not null)
                {
                    await _workflowInstanceStarterManager.UpdateWorkflowStateAsync(workflowInstanceStarter, instance, workflowDefinition);
                    await _instanceStarterRepository.UpdateAsync(workflowInstanceStarter);
                }
            }
        }
    }
}
