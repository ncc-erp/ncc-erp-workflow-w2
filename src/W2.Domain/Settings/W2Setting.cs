using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities;

namespace W2.Settings
{
    public class W2Setting : BasicAggregateRoot<Guid>
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

        [NotMapped]
        public W2SettingValue ValueObject
        {
            get
            {
                return string.IsNullOrEmpty(Value) ? null : JsonConvert.DeserializeObject<W2SettingValue>(Value);
            }
            set
            {
                Value = JsonConvert.SerializeObject(value);
            }
        }
    }
}
