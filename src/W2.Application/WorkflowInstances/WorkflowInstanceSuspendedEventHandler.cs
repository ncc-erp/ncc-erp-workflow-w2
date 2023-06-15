using Elsa.Events;
using Elsa.Models;
using Elsa.Persistence;
using Elsa.Persistence.Specifications;
using Elsa.Persistence.Specifications.WorkflowDefinitions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using W2.Specifications;

namespace W2.WorkflowInstances
{
    public class WorkflowInstanceSuspendedEventHandler : INotificationHandler<WorkflowSuspended>
    {
        private readonly IWorkflowDefinitionStore _workflowDefinitionStore;
        private readonly IWorkflowInstanceStore _workflowInstanceStore;
        private readonly IRepository<WorkflowInstanceStarter, Guid> _instanceStarterRepository;
        private readonly WorkflowInstanceStarterManager _workflowInstanceStarterManager;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public WorkflowInstanceSuspendedEventHandler(
            IWorkflowDefinitionStore workflowDefinitionStore, 
            IWorkflowInstanceStore workflowInstanceStore,
            IRepository<WorkflowInstanceStarter, Guid> instanceStarterRepository,
            WorkflowInstanceStarterManager workflowInstanceStarterManager,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _workflowDefinitionStore = workflowDefinitionStore;
            _workflowInstanceStore = workflowInstanceStore;
            _workflowInstanceStarterManager = workflowInstanceStarterManager;
            _instanceStarterRepository = instanceStarterRepository;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task Handle(WorkflowSuspended notification, CancellationToken cancellationToken)
        {   
            if (!notification.WorkflowExecutionContext.HasBlockingActivities) 
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
                }

                await uow.CompleteAsync();
            }
        }
    }
}
