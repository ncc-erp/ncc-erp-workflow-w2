using System;
using System.Collections.Generic;
using System.Text;
using W2.Tasks;

namespace W2.WorkflowInstances
{
    public class WorkflowInstanceDetailDto
    {
        public string typeRequest { get; set; }
        public object input { get; set; }
        public string workInstanceId { get; set; }
        public List<W2TasksDto> tasks { get; set; }
    }
}
