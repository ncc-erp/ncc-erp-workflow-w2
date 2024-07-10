using System;
using System.Collections.Generic;
using System.Text;

namespace W2.WorkflowInstances
{
    public class ListAllCountingWFHRequestInput
    {
        public int limit { get; set; }
        public int offset { get; set; }
        public DateTime from { get; set; }
        public DateTime to { get; set; }
    }
}
