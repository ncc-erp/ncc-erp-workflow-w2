using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace W2.TaskEmail
{
    public class W2TaskEmail : CreationAuditedEntity<Guid>
    {
        public List<string> EmailTo { get; set; } // List of emails for Task
        public Guid Author { get; set; }
    }
}
