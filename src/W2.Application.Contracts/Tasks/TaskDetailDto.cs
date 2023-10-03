using System;
using System.Collections.Generic;
using System.Text;
using W2.TaskActions;

namespace W2.Tasks
{
    public class TaskDetailDto
    {
        public W2TasksDto Tasks { get; set; }
        public List<string> EmailTo { get; set; }
        public List<TaskActionsDto>? OtherActionSignals { get; set; }
        public object Input { get; set; }
    }
}
