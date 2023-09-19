using Elsa;
using Elsa.Activities.Email;
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
using W2.Signals;
using W2.Tasks;

namespace W2.Activities
{
    [Action(
        Category = "Email",
        DisplayName = "Sender Email and assign email",
        Description = "Send an email message and assign to email.",
        Outcomes = new[] { OutcomeNames.Done, "Unexpected Error" })]
    public class SendMailAndAssign : SendEmail
    {
        private ITaskAppService _taskAppService;
        public SendMailAndAssign(ISmtpService smtpService, 
            IOptions<SmtpOptions> options, 
            IHttpClientFactory httpClientFactory,
            ITaskAppService taskAppService,
            IContentSerializer contentSerializer) 
            : base(smtpService, options, httpClientFactory, contentSerializer)
        {
            _taskAppService = taskAppService;
        }

        public new string From => string.Empty;

        [ActivityInput(Hint = "The approved signal", SupportedSyntaxes = new string[] { "JavaScript", "Liquid" })]
        public string ApproveSignal { get; set; }

        [ActivityInput(Hint = "The reject signal", SupportedSyntaxes = new string[] { "JavaScript", "Liquid" })]
        public string RejectSignal { get; set; }

        [ActivityInput(Hint = "Other action for signal", SupportedSyntaxes = new string[] { "JavaScript", "Liquid" })]
        public string OtherActionSignal { get; set; }

        protected async override ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            if (To == null)
            {
                To = new List<string>();
            }

            var currentUser = context.GetRequestUserVariable();
            var Description = context.ActivityBlueprint.DisplayName;

            foreach (var email in To)
            {
                if (email != null)
                {
                    await _taskAppService.assignTask(email,
                        (Guid)currentUser.Id,
                        context.WorkflowInstance.Id,
                        ApproveSignal.Trim(),
                        RejectSignal.Trim(),
                        OtherActionSignal,
                        Description);
                }
            }

            return await base.OnExecuteAsync(context);
        }
    }
}
