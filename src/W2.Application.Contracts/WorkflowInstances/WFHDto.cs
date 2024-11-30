using System.Collections.Generic;

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
