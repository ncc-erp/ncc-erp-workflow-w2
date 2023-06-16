using Elsa.Events;
using Elsa.Persistence;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using W2.Specifications;

namespace W2.WorkflowInstances
{
    public class WorkflowInstanceSuspendedEventHandler : INotificationHandler<WorkflowSuspended>
    {
        private readonly IWorkflowDefinitionStore _workflowDefinitionStore;
        private readonly IRepository<WorkflowInstanceStarter, Guid> _instanceStarterRepository;
        private readonly WorkflowInstanceStarterManager _workflowInstanceStarterManager;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IDistributedCache<string[]> _distributedCache;

        public WorkflowInstanceSuspendedEventHandler(
            IWorkflowDefinitionStore workflowDefinitionStore,
            IRepository<WorkflowInstanceStarter, Guid> instanceStarterRepository,
            WorkflowInstanceStarterManager workflowInstanceStarterManager,
            IUnitOfWorkManager unitOfWorkManager,
            IDistributedCache<string[]> distributedCache)
        {
            _workflowDefinitionStore = workflowDefinitionStore;
            _workflowInstanceStarterManager = workflowInstanceStarterManager;
            _instanceStarterRepository = instanceStarterRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _distributedCache = distributedCache;
        }

        public async Task Handle(WorkflowSuspended notification, CancellationToken cancellationToken)
        {   
            if (!notification.WorkflowExecutionContext.HasBlockingActivities) 
            {
                return;
            }

            var blockingActivitiesInCache = await _distributedCache.GetAsync(notification.WorkflowExecutionContext.WorkflowInstance.Id);
            var blockingActivities = notification.WorkflowExecutionContext.WorkflowInstance.BlockingActivities.Select(x => x.ActivityId);
            if (blockingActivitiesInCache != null && blockingActivitiesInCache.All(x => blockingActivities.Contains(x)))
            {
                return;
            }

            using (var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false))
            {
                var workflowInstance = notification.WorkflowExecutionContext.WorkflowInstance;
                var workflowDefinition = await _workflowDefinitionStore.FindAsync(new FindWorkflowDefinitionByIdSpecification(workflowInstance.DefinitionId));
                var workflowInstanceStarter = await _instanceStarterRepository.FindAsync(x => x.WorkflowInstanceId == workflowInstance.Id, includeDetails: true);
                if (workflowDefinition != null && workflowInstanceStarter != null)
                {
                    await _workflowInstanceStarterManager.UpdateWorkflowStateAsync(workflowInstanceStarter, workflowInstance, workflowDefinition);
                    await _instanceStarterRepository.UpdateAsync(workflowInstanceStarter);
                }

                await uow.CompleteAsync();
                await _distributedCache.SetAsync(notification.WorkflowExecutionContext.WorkflowInstance.Id, blockingActivities.ToArray());
            }
        }
    }
}
