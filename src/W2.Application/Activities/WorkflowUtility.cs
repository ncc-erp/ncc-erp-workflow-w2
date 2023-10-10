using Elsa.Models;
using Elsa.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace W2.Activities
{
    public class WorkflowUtility
    {
        public static async Task ProcessBlockingActivitiesAsync(ActivityExecutionContext context)
        {
            ICompositeActivityBlueprint parentBlueprint = context.ActivityBlueprint.Parent;
            bool isRoot = parentBlueprint == null;
            HashSet<BlockingActivity> blockingActivities = context.WorkflowExecutionContext.WorkflowInstance.BlockingActivities;
            List<string> blockingActivityIds = blockingActivities.Select((BlockingActivity x) => x.ActivityId).ToList();
            List<string> containedBlockingActivityIds = ((parentBlueprint == null) ? blockingActivityIds : (from x in parentBlueprint.Activities
                                                                                                            where blockingActivityIds.Contains(x.Id)
                                                                                                            select x.Id).ToList());
            IEnumerable<BlockingActivity> containedBlockingActivities = blockingActivities.Where((BlockingActivity x) => containedBlockingActivityIds.Contains(x.ActivityId));
            foreach (BlockingActivity blockingActivity in containedBlockingActivities)
            {
                await context.WorkflowExecutionContext.RemoveBlockingActivityAsync(blockingActivity).ConfigureAwait(continueOnCapturedContext: false);
            }
        }

        public static async Task ProcessScopeActivitiesAsync(ActivityExecutionContext context)
        {
            ICompositeActivityBlueprint parentBlueprint = context.ActivityBlueprint.Parent;
            List<ActivityScope> scopes = context.WorkflowInstance.Scopes.Select((ActivityScope x) => x).Reverse().ToList();
            List<string> scopeIds = scopes.Select((ActivityScope x) => x.ActivityId).ToList();
            List<string> containedScopeActivityIds = ((parentBlueprint == null) ? scopeIds : (from x in parentBlueprint.Activities
                                                                                              where scopeIds.Contains(x.Id)
                                                                                              select x.Id).ToList());
            foreach (string scopeId in containedScopeActivityIds)
            {
                IActivityBlueprint scopeActivity = context.WorkflowExecutionContext.GetActivityBlueprintById(scopeId);
                await context.WorkflowExecutionContext.EvictScopeAsync(scopeActivity).ConfigureAwait(continueOnCapturedContext: false);
                scopes.RemoveAll((ActivityScope x) => x.ActivityId == scopeId);
            }

            context.WorkflowInstance.Scopes = new SimpleStack<ActivityScope>(scopes.AsEnumerable().Reverse());
        }
    }

}
