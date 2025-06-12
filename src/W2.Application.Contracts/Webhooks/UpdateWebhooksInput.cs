using System;
using System.Collections.Generic;

namespace W2.Webhooks
{
    public class UpdateWebhooksInput
    {
        public string WebhookName { get; set; }
        public string Url { get; set; }
        public List<string> EventNames { get; set; }
    }
}