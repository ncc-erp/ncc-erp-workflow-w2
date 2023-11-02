using System;
using System.Collections.Generic;
using System.Text;

namespace W2.Tasks
{
    public class ListTasksInputExternal
    {
        public int MaxResultCount { get; set; }
        public int SkipCount { get; set; }
        public string Email { get; set; }
        public List<W2TaskStatus>? Status { get; set; }
        public List<string>? RequestName { get; set; }
    }
}
