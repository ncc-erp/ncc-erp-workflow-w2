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
using W2.ExternalResources;
using W2.HostedService;
using W2.Signals;
using W2.Tasks;

namespace W2.Activities
{
    [Action(
        Category = "Email",
        DisplayName = "Send email to my branch manager",
        Description = "Send an email message with default sender.",
        Outcomes = new[] { OutcomeNames.Done, "Unexpected Error" })]
    public class SendEmailToMyBranchManager : CustomEmail
    {
        private readonly IExternalResourceAppService _externalResourceAppService;

        public SendEmailToMyBranchManager(ISmtpService smtpService,
            IOptions<SmtpOptions> options,
            IHttpClientFactory httpClientFactory,
            ITaskAppService taskAppService,
            ITaskQueue taskQueue,
            IContentSerializer contentSerializer,
            IExternalResourceAppService externalResourceAppService) : base(smtpService, options, httpClientFactory, taskAppService, taskQueue, contentSerializer)
        {
            _externalResourceAppService = externalResourceAppService;
        }

        public new ICollection<string> Cc { get; set; }
        public new ICollection<string> Bcc { get; set; }

        protected async override ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            if (To == null)
            {
                To = new List<string>();
            }

            var currentUser = context.GetRequestUserVariable();
            var userProjectsResult = await _externalResourceAppService.GetUserBranchInfoAsync(currentUser.Email);
            To.Add(userProjectsResult.HeadOfOfficeEmail);

            return await base.OnExecuteAsync(context);
        }
    }
}
