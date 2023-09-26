using System;
using System.Collections.Generic;
using Volo.Abp.Auditing;
using W2.Tasks;
using W2.TaskActions;

namespace W2.TaskActions
{
    public class TaskActionsDto
    {
        public Guid Id { get; set; }
        public List<string> OtherActionSignals { get; set; } // List of other action signals for Task
        public W2TaskActionsStatus Status { get; set; }

    }
}
