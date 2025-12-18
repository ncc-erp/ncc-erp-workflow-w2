using System;

namespace W2.WorkflowInstances
{
    public class RequestHistoryCreatedEvent
    {
        public WorkflowInstanceStarter Starter { get; set; } = default!;
        public string Email { get; set; } = default!;
    }

    public class RequestHistoryStatusChangedEvent
    {
        public Guid WorkflowInstanceStarterId { get; set; }
        public WorkflowInstancesStatus NewStatus { get; set; }
    }
}
