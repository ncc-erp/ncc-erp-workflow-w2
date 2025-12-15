using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace W2.Webhooks
{
    public interface IWebhookAppService : IApplicationService
    {
        Task<ListResultDto<WebhooksDto>> GetAsync();
        Task<WebhooksDto> GetAsync(Guid id);
        Task<WebhooksDto> CreateAsync(CreateWebhooksInput input);
        Task<WebhooksDto> UpdateAsync(Guid id, UpdateWebhooksInput input);
        Task DeleteAsync(Guid id);
    }


    public interface IWebhookSender
    {
        Task SendCreatedRequest(string eventName, object Payload);
        Task SendFinishedRequest(string eventName, object Payload);
        Task SendAssignedTask(string eventName, object Payload);
    }
}
