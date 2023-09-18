using System;
using System.Collections.Generic;
using System.Text;

namespace W2.Tasks
{
    public class TaskDto
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public W2TaskStatus status { get; set; }
        public DateTime creationTime { get; set; }
    }

    public class TaskDetailDto
    {
        public TaskDto tasks { get; set; }
        public object input { get; set; }
    }
}
