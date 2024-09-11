using System;
using Volo.Abp.Domain.Entities;

namespace W2.Settings
{
    public class W2Setting : BasicAggregateRoot<Guid>
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
