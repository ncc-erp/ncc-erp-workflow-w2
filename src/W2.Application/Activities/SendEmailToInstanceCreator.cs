using Elsa;
using Elsa.Activities.Email.Options;
using Elsa.Activities.Email.Services;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Serialization;
using Elsa.Services.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;
using W2.HostedService;
using W2.Signals;
using W2.Tasks;

namespace W2.Activities
{
    [Action(
        Category = "Email",
        DisplayName = "Send email to instance creator and other",
        Description = "Send an email to current instance creator and other.",
        Outcomes = new[] { OutcomeNames.Done, "Unexpected Error" })]
    public class SendEmailToInstanceCreatorAndOther : CustomEmail
    {
        public SendEmailToInstanceCreatorAndOther(ISmtpService smtpService,
            IOptions<SmtpOptions> options,
            IHttpClientFactory httpClientFactory,
            ITaskAppService taskAppService,
            ITaskQueue taskQueue,
            IContentSerializer contentSerializer)
            : base(smtpService, options, httpClientFactory, taskAppService, taskQueue, contentSerializer)
        {
        }

        public new ICollection<string> Cc { get; set; }
        public new ICollection<string> Bcc { get; set; }

        protected async override ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            var outcomes = new List<string> { OutcomeNames.Done };

            try
            {
                var requestUser = context.GetRequestUserVariable();

                if (base.To == null)
                {
                    base.To = new List<string>();
                }

                if (requestUser != null && !To.Contains(requestUser.Email))
                {
                    base.To.Add(requestUser.Email);
                }

                return await base.OnExecuteAsync(context);
            }
            catch (EntityNotFoundException ex)
            {
                outcomes.Add("Unexpected Error");
                context.JournalData.Add("Error", ex.Message);
                return Outcomes(outcomes);
            }
        }
    }
}
