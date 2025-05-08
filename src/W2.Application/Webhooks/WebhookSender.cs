using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using System;
using Elsa.Persistence;
using W2.Identity;
using W2.TaskEmail;
using System.Collections.Generic;

namespace W2.Webhooks
{
    public class WebhookSender : IWebhookSender, ITransientDependency
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IRepository<W2Webhooks, Guid> _webhookRepository;
        private readonly ILogger<WebhookSender> _logger;
        private readonly IWorkflowDefinitionStore _workflowDefinitionStore;
        private readonly IRepository<W2CustomIdentityUser, Guid> _userRepository;
        private readonly IRepository<W2TaskEmail, Guid> _taskEmailRepository;
        private readonly Dictionary<string, Func<object, Task>> _eventHandlers;

        public WebhookSender(
            IHttpClientFactory httpClientFactory,
            IRepository<W2Webhooks, Guid> webhookRepository,
            ILogger<WebhookSender> logger,
            IWorkflowDefinitionStore workflowDefinitionStore,
            IRepository<W2CustomIdentityUser, Guid> userRepository,
            IRepository<W2TaskEmail, Guid> taskEmailRepository
        )
        {
            _httpClientFactory = httpClientFactory;
            _webhookRepository = webhookRepository;
            _logger = logger;
            _workflowDefinitionStore = workflowDefinitionStore;
            _userRepository = userRepository;
            _taskEmailRepository = taskEmailRepository;
        }
        public async Task SendCreatedRequest(string eventName, object payload)
        {
            var (workflowName, creatorName, _) = await ExtractCommonInfo(payload);
            string messageText = GenerateMessage(workflowName, creatorName, "Created");
            await SendToWebhooks(eventName, messageText);
        }

        public async Task SendFinishedRequest(string eventName, object payload)
        {
            var (workflowName, creatorName, _) = await ExtractCommonInfo(payload);
            string messageText = GenerateMessage(workflowName, creatorName, "Finished");
            await SendToWebhooks(eventName, messageText);
        }

        public async Task SendAssignedTask(string eventName, object payload)
        {
            var (workflowName, creatorName, taskId) = await ExtractCommonInfo(payload, true);
            _logger.LogInformation($"TaskId: {taskId}");

            string assigneeName = "Unknown User";
            var taskIdString = taskId != null ? taskId.ToString() : null;
            var taskEmail = await _taskEmailRepository.FirstOrDefaultAsync(x => x.TaskId == taskIdString);
            if (!string.IsNullOrEmpty(taskEmail?.Email))
            {
                assigneeName = taskEmail.Email;
            }

            string messageText = GenerateMessage(workflowName, creatorName, "Assigned", assigneeName);
            await SendToWebhooks(eventName, messageText);
        }

        private async Task<(string workflowName, string creatorName, object taskId)> ExtractCommonInfo(object payload, bool includeTaskId = false)
        {
            string workflowName = "Unknown Request";
            string creatorName = "Unknown User";
            object taskId = null;

            var creatorId = payload.GetType().GetProperty("CreatorId")?.GetValue(payload) ??
                            payload.GetType().GetProperty("AuthorId")?.GetValue(payload);
            var workflowDefinitionId = payload.GetType().GetProperty("WorkflowDefinitionId")?.GetValue(payload);
            if (includeTaskId)
            {
                taskId = payload.GetType().GetProperty("TaskId")?.GetValue(payload);
            }

            if (creatorId is Guid creatorGuid)
            {
                var user = await _userRepository.FindAsync(creatorGuid);
                if (user != null) creatorName = user.Name;
            }

            if (workflowDefinitionId is string workflowId)
            {
                var workflowDefinition = await _workflowDefinitionStore.FindAsync(
                    new Elsa.Persistence.Specifications.WorkflowDefinitions.WorkflowDefinitionIdSpecification(workflowId));
                if (workflowDefinition != null) workflowName = workflowDefinition.Name;
            }

            return (workflowName, creatorName, taskId);
        }

        private async Task SendToWebhooks(string eventName, string messageText)
        {
            var webhooks = await _webhookRepository.GetListAsync();
            string json = FormatWebhookPayload(messageText);
            foreach (var webhook in webhooks)
            {
                if (webhook.EventNames == null || !webhook.EventNames.Contains(eventName))
                {
                    continue;
                }

                try
                {
                    var client = _httpClientFactory.CreateClient();
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(webhook.Url, content);
                    var responseText = await response.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error sending webhook to {webhook.Url}: {ex.Message}");
                }
            }
        }

        private string FormatWebhookPayload(string messageText)
        {
            var formattedPayload = new
            {
                type = "hook",
                message = new
                {
                    t = messageText,
                    mk = new[]
                    {
                    new { type = "pre", s = 0, e = messageText.Length }
                }
                }
            };
            return JsonConvert.SerializeObject(formattedPayload);
        }
        private string GenerateMessage(string workflowName, string creatorName, string eventType, string assigneeName = null)
        {
            return eventType switch
            {
                "Created" => $"[Request] {workflowName} is created by {creatorName}",
                "Finished" => $"[Request] {workflowName} created by {creatorName} is finished",
                "Assigned" when assigneeName != null => $"[Request] {workflowName} created by {creatorName} is assigned to {assigneeName}",
                _ => "[Request] Unknown event"
            };
        }
    }
}
