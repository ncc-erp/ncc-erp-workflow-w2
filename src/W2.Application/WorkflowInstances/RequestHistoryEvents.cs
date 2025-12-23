using System;

namespace W2.WorkflowInstances
{
    public class RequestCreatedEvent
    {
        public WorkflowInstanceStarter Starter { get; set; } = default!;
        public string Email { get; set; } = default!;
    }

    public class RequestStatusChangedEvent
    {
        public Guid WorkflowInstanceStarterId { get; set; }
        public WorkflowInstancesStatus NewStatus { get; set; }
    }
}
