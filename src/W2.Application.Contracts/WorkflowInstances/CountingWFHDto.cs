using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Auditing;

namespace W2.WorkflowInstances
{
    public class CountingWFHDto
    {
        public string email { get; set; }
        public int totalRemoteCount { get; set; }
        public int totalRemoteDay { get; set; }
        public string branch { get; set; }
    }
}
