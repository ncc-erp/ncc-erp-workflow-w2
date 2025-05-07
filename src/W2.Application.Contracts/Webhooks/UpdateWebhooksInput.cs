using System;

namespace W2.Webhooks
{
    public class UpdateWebhooksInput
    {
        public string EventName { get; set; }
        public string Url { get; set; }
    }
}