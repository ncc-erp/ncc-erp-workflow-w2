using System;
using Volo.Abp.Domain.Entities;

namespace W2.TaskEmail
{
    public class W2TaskEmail : BasicAggregateRoot<Guid>
    {
        public string Email { get; set; } // email for Task
        public string TaskId { get; set; }
    }
}
