using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities;
public class W2Webhooks : IEntity<Guid>
{
    public string WebhookName { get; set; }
    public List<string> EventNames { get; set; }
    public string Url { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreationTime { get; set; }

    public Guid Id { get; set; }
    public W2Webhooks(
        Guid id,
        string url,
        string webhookName,
        List<string> eventNames
    )
    {
        Id = id;
        Url = url;
        WebhookName = webhookName;
        CreationTime = DateTime.UtcNow;
        IsActive = true;
        EventNames = new List<string>(eventNames);
    }
    public object[] GetKeys()
    {
        return new object[] { Id };
    }
}
