using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using W2.Authorization.Attributes;
using W2.Constants;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace W2.Webhooks
{
    [Route("api/app/webhooks")]
    public class WebhookAppService : W2AppService, IWebhookAppService
    {
        private readonly IRepository<W2Webhooks, Guid> _webhookRepository;
        public const int MaxWebhookNameLength = 250;
        public const int MaxUrlLength = 2048;

        public WebhookAppService(
            IRepository<W2Webhooks, Guid> webhookRepository
            )
        {
            _webhookRepository = webhookRepository;
        }

        [HttpGet]
        [RequirePermission(W2ApiPermissions.ViewListWebhooks)]
        public async Task<ListResultDto<WebhooksDto>> GetAsync()
        {
            var query = await _webhookRepository.GetQueryableAsync();
            var webhooks = await query.ToListAsync();
            return new ListResultDto<WebhooksDto>(
                ObjectMapper.Map<List<W2Webhooks>, List<WebhooksDto>>(webhooks)
            );
        }

        [HttpGet("{id}")]
        [RequirePermission(W2ApiPermissions.ViewListWebhooks)]
        public async Task<WebhooksDto> GetAsync(Guid id)
        {
            var webhook = await _webhookRepository.FirstOrDefaultAsync(w => w.Id == id)
                          ?? throw new UserFriendlyException("Webhook not found.");

            return ObjectMapper.Map<W2Webhooks, WebhooksDto>(webhook);
        }

        [HttpPut("{id}")]
        [RequirePermission(W2ApiPermissions.UpdateWebhook)]
        public async Task<WebhooksDto> UpdateAsync(Guid id, UpdateWebhooksInput input)
        {
            var invalidEvents = input.EventNames
                .Where(e => !WebhookEvents.ValidEvents.Contains(e, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (invalidEvents.Any())
            {
                throw new UserFriendlyException($"Invalid event names: {string.Join(", ", invalidEvents)}");
            }
            if (input.WebhookName.Length > MaxWebhookNameLength)
                throw new UserFriendlyException("WebhookName is too long.");

            if (input.Url.Length > MaxUrlLength)
                throw new UserFriendlyException("URL is too long.");

            if (string.IsNullOrWhiteSpace(input.Url) || string.IsNullOrWhiteSpace(input.WebhookName))
                throw new UserFriendlyException("URL and WebhookName cannot be empty.");

            if (input.EventNames == null || !input.EventNames.Any())
                throw new UserFriendlyException("At least one EventName is required.");

            input.EventNames = input.EventNames
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e.Trim())
                .Distinct()
                .ToList();

            var query = await _webhookRepository.GetQueryableAsync();

            var existUrl = await query.FirstOrDefaultAsync(w =>
                w.Id != id &&
                w.Url == input.Url.Trim() &&
                w.WebhookName == input.WebhookName.Trim());

            if (existUrl != null)
            {
                throw new UserFriendlyException("Webhook with this URL already exists.");
            }
            var existWebhook = await query.FirstOrDefaultAsync(w =>
                w.Id != id &&
                w.WebhookName == input.WebhookName.Trim());

            if (existWebhook != null)
            {
                throw new UserFriendlyException("Webhook Name already exists.");
            }



            var webhook = await query.FirstOrDefaultAsync(w => w.Id == id)
                          ?? throw new UserFriendlyException("Webhook not found.");

            webhook.Url = input.Url.Trim();
            webhook.WebhookName = input.WebhookName.Trim();
            webhook.EventNames = input.EventNames;

            await _webhookRepository.UpdateAsync(webhook);

            return ObjectMapper.Map<W2Webhooks, WebhooksDto>(webhook);
        }
        [HttpPost]
        [RequirePermission(W2ApiPermissions.CreateWebhook)]
        public async Task<WebhooksDto> CreateAsync(CreateWebhooksInput input)
        {
            var invalidEvents = input.EventNames
                .Where(e => !WebhookEvents.ValidEvents.Contains(e, StringComparer.OrdinalIgnoreCase))
                .ToList();
            if (invalidEvents.Any())
            {
                throw new UserFriendlyException($"Invalid event names: {string.Join(", ", invalidEvents)}");
            }
            if (input.Url.Length > MaxUrlLength)
                throw new UserFriendlyException("URL is too long.");

            if (string.IsNullOrWhiteSpace(input.Url) || string.IsNullOrWhiteSpace(input.WebhookName))
                throw new UserFriendlyException("URL and WebhookName cannot be empty.");

            if (input.EventNames == null || !input.EventNames.Any())
                throw new UserFriendlyException("At least one EventName is required.");

            input.EventNames = input.EventNames
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e.Trim())
                .Distinct()
                .ToList();

            var query = await _webhookRepository.GetQueryableAsync();

            var existUrl = await query.FirstOrDefaultAsync(w =>
                w.Url == input.Url.Trim() &&
                w.WebhookName == input.WebhookName.Trim());

            if (existUrl != null)
                throw new UserFriendlyException("Webhook with this URL already exists.");

            var existWebhook = await query.FirstOrDefaultAsync(w =>
                w.WebhookName == input.WebhookName.Trim());

            if (existWebhook != null)
                throw new UserFriendlyException("Webhook Name already exists.");

            var webhook = new W2Webhooks(
                GuidGenerator.Create(),
                input.Url.Trim(),
                input.WebhookName.Trim(),
                input.EventNames
            );

            await _webhookRepository.InsertAsync(webhook);

            return ObjectMapper.Map<W2Webhooks, WebhooksDto>(webhook);
        }


        [HttpDelete("{id}")]
        [RequirePermission(W2ApiPermissions.DeleteWebhook)]
        public async Task DeleteAsync(Guid id)
        {
            var webhook = await _webhookRepository.GetAsync(id);
            if (webhook == null)
            {
                throw new UserFriendlyException("Webhook not found.");
            }
            await _webhookRepository.DeleteAsync(webhook);
        }
    }
}
