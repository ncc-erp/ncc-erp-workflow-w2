using Elsa.Events;
using Elsa.Models;
using Elsa.Persistence;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using W2.Specifications;

namespace W2.WorkflowInstances
{
    public class WorkflowInstanceEndedEventHandler : INotificationHandler<WorkflowFaulted>, INotificationHandler<WorkflowCompleted>, INotificationHandler<WorkflowCancelled>
    {
        private readonly IWorkflowDefinitionStore _workflowDefinitionStore;
        private readonly IWorkflowInstanceStore _workflowInstanceStore;
        private readonly IRepository<WorkflowInstanceStarter, Guid> _instanceStarterRepository;
        private readonly WorkflowInstanceStarterManager _workflowInstanceStarterManager;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public WorkflowInstanceEndedEventHandler(
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

        public async Task Handle(WorkflowCancelled notification, CancellationToken cancellationToken)
        {
            await EndedWorkflow(notification.WorkflowExecutionContext.WorkflowInstance);
        }

        public async Task Handle(WorkflowFaulted notification, CancellationToken cancellationToken)
        {
            await EndedWorkflow(notification.WorkflowExecutionContext.WorkflowInstance);
        }

        public async Task Handle(WorkflowCompleted notification, CancellationToken cancellationToken)
        {
            await EndedWorkflow(notification.WorkflowExecutionContext.WorkflowInstance);
        }

        private async Task EndedWorkflow(WorkflowInstance workflowInstance)
        {
            using (var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false))
            {
                var workflowDefinition = await _workflowDefinitionStore.FindAsync(new FindWorkflowDefinitionByIdSpecification(workflowInstance.DefinitionId));
                var workflowInstanceStarter = await _instanceStarterRepository.FindAsync(x => x.WorkflowInstanceId == workflowInstance.Id, includeDetails: true);
                if (workflowDefinition != null && workflowInstanceStarter != null)
                {
                    await _workflowInstanceStarterManager.RefreshStateAsync(workflowInstanceStarter);
                    await _workflowInstanceStarterManager.UpdateWorkflowStateAsync(workflowInstanceStarter, workflowInstance, workflowDefinition);
                    await _instanceStarterRepository.UpdateAsync(workflowInstanceStarter);
                }

                await uow.CompleteAsync();
            }
        }
    }
}
