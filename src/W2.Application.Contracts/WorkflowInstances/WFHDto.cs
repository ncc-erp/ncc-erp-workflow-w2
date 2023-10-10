using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Auditing;

namespace W2.WorkflowInstances
{
    public class WFHDto
    {
        public string email { get; set; }
        public int totalDays { get; set; }
        public int totalPosts { get; set; }
        public int totalMissingPosts { get; set; }
        public object requests { get; set; }
        public List<string> requestDates { get; set; }
        public object posts { get; set; }
    }
}
