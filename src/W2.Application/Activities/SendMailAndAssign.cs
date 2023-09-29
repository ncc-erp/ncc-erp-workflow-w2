using Elsa;
using Elsa.Activities.Email;
using Elsa.Activities.Email.Options;
using Elsa.Activities.Email.Services;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Serialization;
using Elsa.Services.Models;
using Humanizer;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Volo.Abp;
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

        [ActivityInput(Hint = "Other action for signal", UIHint = "multi-text", DefaultSyntax = "Json", SupportedSyntaxes = new string[] { "Json", "JavaScript" })]
        public List<string> OtherActionSignals { get; set; }

        [ActivityInput(Hint = "Email to assign", UIHint = "multi-text", DefaultSyntax = "Json", SupportedSyntaxes = new string[] { "Json", "JavaScript" })]
        public List<string> AssignTo { get; set; }

        protected async override ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            if (To == null && AssignTo == null)
            {
                throw new UserFriendlyException("Exception:No Email address To send");
            }
            List<string> EmailTo = new List<string>();

            if (AssignTo != null)
            {
                foreach (string email in AssignTo)
                {
                    EmailTo.Add(email);
                }
            } else
            {
                foreach (string email in To)
                {
                    EmailTo.Add(email);
                }
            }

            var currentUser = context.GetRequestUserVariable();
            var Description = context.ActivityBlueprint.DisplayName;

            var input = new AssignTaskInput
            {
                UserId = (Guid)currentUser.Id,
                WorkflowInstanceId = context.WorkflowInstance.Id,
                ApproveSignal = ApproveSignal.Trim(),
                RejectSignal = RejectSignal.Trim(),
                Description = Description,
                EmailTo = EmailTo,
                OtherActionSignals = OtherActionSignals
            };

            await _taskAppService.assignTask(input);

            return await base.OnExecuteAsync(context);
        }
    }
}
