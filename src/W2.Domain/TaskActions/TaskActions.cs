using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using W2.Tasks;

namespace W2.TaskActions
{
    public class W2TaskActions : CreationAuditedEntity<Guid>
    {
        public Guid Author { get; set; }
        public List<string> OtherActionSignals { get; set; } // List of other action signals for Task
        public string UpdatedBy { get; set; } // Task description
        public W2TaskActionsStatus Status { get; set; }
    }
}
