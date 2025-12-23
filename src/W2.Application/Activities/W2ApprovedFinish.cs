using Elsa.Activities.ControlFlow;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Models;
using Elsa.Services.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Local;
using W2.Tasks;
using W2.WorkflowInstances;

namespace W2.Activities
{
    [Activity(
        Category = "Workflows",
        DisplayName = "W2 Approved Finish",
        Description = "Update Assign Task Status, Workflow Status and Removes any blocking activities from the current container (workflow or composite activity).", 
        Outcomes = new string[] { })]
    public class W2ApprovedFinish : Finish
    {
        private IRepository<W2Task, Guid> _taskRepository;
        private readonly IRepository<WorkflowInstanceStarter, Guid> _instanceStarterRepository;
        private readonly ILocalEventBus _localEventBus;
        private readonly ILogger<W2ApprovedFinish> _logger;

        public W2ApprovedFinish(
            IRepository<W2Task, Guid> taskRepository,
            ILogger<W2ApprovedFinish> logger,
            IRepository<WorkflowInstanceStarter, Guid> instanceStarterRepository,
            ILocalEventBus localEventBus)
        {
            _taskRepository = taskRepository;
            _instanceStarterRepository = instanceStarterRepository;
            _localEventBus = localEventBus;
            _logger = logger;
        }

        protected async override ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            _logger.LogInformation("start OnExecuteAsync finished");
            string workflowInstanceId = context.WorkflowInstance.Id;
            
            _logger.LogInformation("start OnExecuteAsync finished id: " + workflowInstanceId);
            // update status for workflow task
            var tasksToApprove = await _taskRepository
                .GetListAsync(x => x.WorkflowInstanceId == workflowInstanceId && x.Status == W2TaskStatus.Pending);

            _logger.LogInformation("start OnExecuteAsync finished tasks length: " + tasksToApprove.Count);
            tasksToApprove.ForEach(task =>
            {
                task.Status = W2TaskStatus.Approve;
                task.UpdatedBy = "W2 Workflow";
            });

            await _taskRepository.UpdateManyAsync(tasksToApprove);
            _logger.LogInformation("OnExecuteAsync finished done UpdateManyAsync: " + tasksToApprove.Count);

            // update status for workflow instance stater 
            var myWorkflow = await _instanceStarterRepository.FirstOrDefaultAsync(x => x.WorkflowInstanceId == workflowInstanceId);
            myWorkflow.Status = WorkflowInstancesStatus.Approved;
            await _instanceStarterRepository.UpdateAsync(myWorkflow);
            
            // Emit event to update history status
            await _localEventBus.PublishAsync(new RequestStatusChangedEvent
            {
                WorkflowInstanceStarterId = myWorkflow.Id,
                NewStatus = WorkflowInstancesStatus.Approved
            });
            
            _logger.LogInformation("OnExecuteAsync finished done _instanceStarterRepository UpdateManyAsync: " + myWorkflow.Id.ToString());

            // Handler Blocking Activities
            await WorkflowUtility.ProcessBlockingActivitiesAsync(context);

            // Handler Scope Activities
            await WorkflowUtility.ProcessScopeActivitiesAsync(context);

            // Finish process
            Output = new FinishOutput(ActivityOutput, OutcomeNames);
            context.LogOutputProperty(this, "Output", Output);

            ICompositeActivityBlueprint parentBlueprint = context.ActivityBlueprint.Parent;
            bool isRoot = parentBlueprint == null;
            if (isRoot)
            {
                context.WorkflowExecutionContext.ClearScheduledActivities();
            }

            return Noop();
        }
    }
}
