using Elsa.Models;
using Elsa.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Identity;
using W2.WorkflowInstanceStates;

namespace W2.WorkflowInstances
{
    public class WorkflowInstanceStarterManager: DomainService
    {
        private readonly IRepository<WorkflowInstanceStarter, Guid> _instanceStarterRepository;
        private readonly IRepository<WorkflowInstanceStakeHolder> _instanceStakeHolderRepository;
        private readonly IRepository<WorkflowInstanceState> _instanceStateRepository;
        private readonly IIdentityUserRepository _userRepository;

        public WorkflowInstanceStarterManager(
           IRepository<WorkflowInstanceStakeHolder> instanceStakeHolderRepository,
           IRepository<WorkflowInstanceState> instanceStateRepository,
           IRepository<WorkflowInstanceStarter, Guid> instanceStarterRepository,
           IIdentityUserRepository userRepository)
        {
            _instanceStakeHolderRepository = instanceStakeHolderRepository;
            _instanceStateRepository = instanceStateRepository;
            _instanceStarterRepository = instanceStarterRepository;
            _userRepository = userRepository;
        }

        public async Task UpdateWorkflowStateAsync(WorkflowInstanceStarter workflowInstanceStarter, WorkflowInstance workflowInstance, WorkflowDefinition workflowDefinition)
        {
            await RefreshStateAsync(workflowInstanceStarter);
            workflowInstanceStarter.States = new List<WorkflowInstanceState>();

            if (workflowInstance.WorkflowStatus == WorkflowStatus.Finished)
            {
                var lastExecutedActivity = workflowDefinition.Activities.FirstOrDefault(x => x.ActivityId == workflowInstance.LastExecutedActivityId);
                workflowInstanceStarter.FinalStatus = GetFinalStatus(lastExecutedActivity);
            }
            else
            {
                workflowInstanceStarter.FinalStatus = WorkflowFinalStatus.None;
            }

            var blockingActivityIds = workflowInstance.BlockingActivities.Select(x => x.ActivityId);
            foreach (var blockingActitvity in workflowInstance.BlockingActivities)
            {
                var connection = workflowDefinition.Connections.FirstOrDefault(x => x.TargetActivityId == blockingActitvity.ActivityId);

                var parentActivity = workflowDefinition.Activities.FirstOrDefault(x => x.ActivityId == connection?.SourceActivityId);
                if (parentActivity?.Type == "Fork")
                {
                    var childNodes = workflowDefinition.Connections.Where(x => x.SourceActivityId == parentActivity.ActivityId && workflowInstance.ActivityData.ContainsKey(x.TargetActivityId))
                                                                   .Select(x => x.TargetActivityId);
                    if (!childNodes.All(x => blockingActivityIds.Contains(x)))
                    {
                        continue;
                    }

                    if (workflowInstanceStarter.States.Select(x => x.StateName).Contains(parentActivity.DisplayName))
                    {
                        continue;
                    }

                    connection = workflowDefinition.Connections.FirstOrDefault(x => x.TargetActivityId == parentActivity.ActivityId);

                    var parentForkActivity = workflowDefinition.Activities.FirstOrDefault(x => x.ActivityId == connection.SourceActivityId);

                    workflowInstance.ActivityData.TryGetValue(parentForkActivity.ActivityId, out var data);
                    while (data != null && !data.ContainsKey("To") && parentForkActivity != null)
                    {
                        connection = workflowDefinition.Connections.FirstOrDefault(x => x.TargetActivityId == parentForkActivity.ActivityId);
                        parentForkActivity = workflowDefinition.Activities.FirstOrDefault(x => x.ActivityId == connection.SourceActivityId);

                        workflowInstance.ActivityData.TryGetValue(parentForkActivity.ActivityId, out data);
                    }

                    if (data != null && data.ContainsKey("To"))
                    {
                        var workflowInstanceState = new WorkflowInstanceState
                        {
                            StateName = parentActivity.DisplayName,
                            StakeHolders = new List<WorkflowInstanceStakeHolder>(),
                            WorkflowInstanceStarter = workflowInstanceStarter
                        };

                        foreach (var email in (List<string>)data["To"])
                        {
                            var user = await _userRepository.FindByNormalizedEmailAsync(email.ToUpper());

                            if (user != null && !workflowInstanceState.StakeHolders.Select(x => x.User.Email).Contains(email))
                            {
                                var workflowInstanceStakeholder = new WorkflowInstanceStakeHolder
                                {
                                    WorkflowInstanceState = workflowInstanceState,
                                    User = user
                                };

                                workflowInstanceState.StakeHolders.Add(workflowInstanceStakeholder);
                                await _instanceStakeHolderRepository.InsertAsync(workflowInstanceStakeholder);
                            }
                        }

                        await _instanceStateRepository.InsertAsync(workflowInstanceState);
                        workflowInstanceStarter.States.Add(workflowInstanceState);
                    }
                }
            }
        }

        public async Task RefreshStateAsync(WorkflowInstanceStarter workflowInstanceStarter)
        {
            if (workflowInstanceStarter.States.Any())
            {
                workflowInstanceStarter.States.Clear();

                await _instanceStakeHolderRepository.DeleteManyAsync(workflowInstanceStarter.States.SelectMany(x => x.StakeHolders));
                await _instanceStateRepository.DeleteManyAsync(workflowInstanceStarter.States);
            }
        }

        public async Task EndWorkflow(WorkflowInstanceStarter workflowInstanceStarter, WorkflowInstance workflowInstance, WorkflowDefinition workflowDefinition)
        {
            await RefreshStateAsync(workflowInstanceStarter);
            if (workflowInstance.WorkflowStatus == WorkflowStatus.Finished)
            {
                var lastExecutedActivity = workflowDefinition.Activities.FirstOrDefault(x => x.ActivityId == workflowInstance.LastExecutedActivityId);
                workflowInstanceStarter.FinalStatus = GetFinalStatus(lastExecutedActivity);
            }
        }

        private WorkflowFinalStatus GetFinalStatus(ActivityDefinition activityDefinition)
        {
            return activityDefinition == null ? WorkflowFinalStatus.None : (activityDefinition.DisplayName.ToLower().Contains("reject") ? WorkflowFinalStatus.Rejected : WorkflowFinalStatus.Approved);
        }
    }
}
