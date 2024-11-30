using System;
using Volo.Abp.Domain.Entities;

namespace W2.Komu
{
    public class W2KomuMessageLogs : BasicAggregateRoot<Guid>
    {
        public Guid? Id { get; set; }
        public string SendTo { get; set; }
        public string Message { get; set; }
        public int Status { get; set; }
        public string SystemResponse { get; set; }
        public DateTime CreationTime { get; set; }
        public Guid? CreatorId { get; set; }
    }
}
