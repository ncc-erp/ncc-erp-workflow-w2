using System;
using Volo.Abp.Application.Dtos;

public class WebhooksDto : EntityDto<Guid>
{
    public string EventName { get; set; }
    public string Url { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreationTime { get; set; }

    public new Guid Id { get; set; }
}