using System;

namespace W2.TaskActions
{
    public class TaskActionsDto
    {
        public string? OtherActionSignal { get; set; } // other action signals for Task
        public W2TaskActionsStatus? Status { get; set; }

    }
}