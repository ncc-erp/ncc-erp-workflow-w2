using System;
using System.Collections.Generic;
using System.Text;

namespace W2.WorkflowInstances
{
    public enum WorkflowInstancesStatus
    {
        Pending,
        Approved,
        Rejected,
        Canceled,
        All = -1
    }
}
