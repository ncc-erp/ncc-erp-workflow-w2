using Elsa.Activities.ControlFlow;
using Elsa.Activities.Email.Services;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Models;
using Elsa.Services;
using Elsa.Services.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using W2.Tasks;
using W2.WorkflowInstances;

namespace W2.Activities
{
    [Activity(
        Category = "Workflows",
        DisplayName = "W2 Approved Pre Finish",
        Description = "Update Assign Task Status, Workflow Status and Removes any blocking activities from the current container (workflow or composite activity).", 
        Outcomes = new string[] { })]
    public class W2ApprovedPreFinish : Activity
    {
        private IRepository<W2Task, Guid> _taskRepository;
        private readonly IRepository<WorkflowInstanceStarter, Guid> _instanceStarterRepository;
        private readonly ILogger<W2ApprovedFinish> _logger;

        public W2ApprovedPreFinish(
            IRepository<W2Task, Guid> taskRepository,
            ILogger<W2ApprovedFinish> logger,
            IRepository<WorkflowInstanceStarter, Guid> instanceStarterRepository)
        {
            _taskRepository = taskRepository;
            _instanceStarterRepository = instanceStarterRepository;
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

            List<string> outcomes = new List<string> { "Done" };

            return Outcomes(outcomes);
        }
    }
}
