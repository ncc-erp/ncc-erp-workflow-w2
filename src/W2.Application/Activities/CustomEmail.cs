using Elsa;
using Elsa.Activities.Email;
using Elsa.Activities.Email.Options;
using Elsa.Activities.Email.Services;
using Elsa.Attributes;
using Elsa.Serialization;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;

namespace W2.Activities
{
    [Action(
        Category = "Email",
        DisplayName = "Default Sender Email",
        Description = "Send an email message with default sender.",
        Outcomes = new[] { OutcomeNames.Done, "Unexpected Error" })]
    public class CustomEmail : SendEmail
    {
        public CustomEmail(ISmtpService smtpService, 
            IOptions<SmtpOptions> options, 
            IHttpClientFactory httpClientFactory, 
            IContentSerializer contentSerializer) 
            : base(smtpService, options, httpClientFactory, contentSerializer)
        {
        }

        public new string From => string.Empty;
    }
}
