using System;
using System.Collections.Generic;
using System.Text;

namespace W2.WorkflowInstances
{
    public class ListAllWFHRequestInput
    {
        public int MaxResultCount { get; set; }
        public int SkipCount { get; set; }
        public string Sorting { get; set; }
        public string KeySearch { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
