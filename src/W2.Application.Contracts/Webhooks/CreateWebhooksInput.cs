using System;

namespace W2.Webhooks
{
    public class CreateWebhooksInput
    {
        public string EventName { get; set; }
        public string Url { get; set; }
    }
}