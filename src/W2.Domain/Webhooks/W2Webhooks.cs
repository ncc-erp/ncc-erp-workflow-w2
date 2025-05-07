using System;
using Volo.Abp.Domain.Entities;
public class W2Webhooks : IEntity<Guid>
{
    public string EventName { get; set; }
    public string Url { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreationTime { get; set; }

    public Guid Id { get; set; }
    public W2Webhooks(
        Guid id,
        string url,
        string eventName = null
    )
    {
        Id = id;
        Url = url;
        EventName = eventName;
        CreationTime = DateTime.UtcNow;
        IsActive = true;
    }
    public object[] GetKeys()
    {
        return new object[] { Id };
    }
}
