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
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Volo.Abp.Users;
using W2.ExternalResources;

namespace W2.Activities
{
    [Action(
        Category = "Email",
        DisplayName = "Send email to my PM",
        Description = "Send an email message with default sender.",
        Outcomes = new[] { OutcomeNames.Done, "Unexpected Error" })]
    public class SendEmailToMyPM : CustomEmail
    {
        private readonly IProjectClientApi _projectClientApi;
        private readonly ICurrentUser _currentUser;

        public SendEmailToMyPM(ISmtpService smtpService,
            IOptions<SmtpOptions> options,
            IHttpClientFactory httpClientFactory,
            IContentSerializer contentSerializer,
            IProjectClientApi projectClientApi,
            ICurrentUser currentUser) : base(smtpService, options, httpClientFactory, contentSerializer)
        {
            _projectClientApi = projectClientApi;
            _currentUser = currentUser;
        }

        public new ICollection<string> Cc { get; set; }
        public new ICollection<string> Bcc { get; set; }

        protected async override ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            if (To == null)
            {
                To = new List<string>();
            }

            var userProjectsResult = await _projectClientApi.GetUserProjectsAsync(_currentUser.Email);
            if (userProjectsResult?.Result != null)
            {
                userProjectsResult.Result
                    .Select(i => i.PM?.EmailAddress)
                    .Where(i => !string.IsNullOrWhiteSpace(i))
                    .ToList()
                    .ForEach(email => To.Add(email));
            }

            return await base.OnExecuteAsync(context);
        }
    }
}
