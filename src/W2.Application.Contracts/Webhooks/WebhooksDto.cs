using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

public class WebhooksDto : EntityDto<Guid>
{
    public string WebhookName { get; set; }
    public List<string> EventNames { get; set; }
    public string Url { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreationTime { get; set; }

    public new Guid Id { get; set; }
}