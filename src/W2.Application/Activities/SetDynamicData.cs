using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Services;
using Elsa.Services.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using W2.Permissions;
using W2.Scripting;
using W2.Tasks;

namespace W2.Activities
{
    [Activity(
        DisplayName = "Set Dynamic Data",
        Description = "Set Dynamic Data on the workflow.",
        Category = "Primitives",
        Outcomes = new string[] { "Done" })]
    public class SetDynamicData : Activity
    {
        private readonly ITaskAppService _taskAppService;
        public SetDynamicData(ITaskAppService taskAppService)
        {
            _taskAppService = taskAppService;
        }

        protected async override ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            List<DynamicDataDto> DynamicDataByTask = await _taskAppService.GetDynamicRawData(new TaskDynamicDataInput
            {
                WorkflowInstanceId = context.WorkflowInstance.Id,
            });

            context.SetVariable("DynamicDataByTask", DynamicDataByTask);

            return Done();
        }
    }
}
