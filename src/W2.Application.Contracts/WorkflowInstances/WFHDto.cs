using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Auditing;

namespace W2.WorkflowInstances
{
    public class WFHDto
    {
        public string UserRequestName { get; set; }
        public int Totaldays { get; set; }
        public int Totalposts { get; set; }
        public object Requests { get; set; }
        public object Posts { get; set; }
    }
}
