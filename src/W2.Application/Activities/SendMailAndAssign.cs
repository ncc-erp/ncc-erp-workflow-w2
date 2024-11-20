using Elsa;
using Elsa.Activities.Email;
using Elsa.Activities.Email.Options;
using Elsa.Activities.Email.Services;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Serialization;
using Elsa.Services.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Volo.Abp;
using W2.HostedService;
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
        private readonly ITaskQueue _taskQueue;

        public SendMailAndAssign(ISmtpService smtpService, 
            IOptions<SmtpOptions> options, 
            IHttpClientFactory httpClientFactory,
            ITaskAppService taskAppService,
            ITaskQueue taskQueue,
            IContentSerializer contentSerializer) 
            : base(smtpService, options, httpClientFactory, contentSerializer)
        {
            _taskAppService = taskAppService;
            _taskQueue = taskQueue;
        }

        public new string From => string.Empty;

        [ActivityInput(Hint = "The approved signal", SupportedSyntaxes = new string[] { "JavaScript", "Liquid" })]
        public string ApproveSignal { get; set; }

        [ActivityInput(Hint = "The reject signal", SupportedSyntaxes = new string[] { "JavaScript", "Liquid" })]
        public string RejectSignal { get; set; }

        [ActivityInput(Hint = "The dynamic form data", SupportedSyntaxes = new string[] { "JavaScript", "Liquid" })]
        public string DynamicActionData { get; set; }

        [ActivityInput(Hint = "Other action for signal", UIHint = "multi-text", DefaultSyntax = "Json", SupportedSyntaxes = new string[] { "Json", "JavaScript" })]
        public List<string> OtherActionSignals { get; set; }

        [ActivityInput(Hint = "Email to assign", UIHint = "multi-text", DefaultSyntax = "Json", SupportedSyntaxes = new string[] { "Json", "JavaScript" })]
        public List<string> AssignTo { get; set; }

        protected async override ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            if (To == null)
            {
                throw new UserFriendlyException("Exception:No Email address To send");
            }
            List<string> EmailTo = new List<string>();

            if (AssignTo != null && AssignTo.Count > 0)
            {
                foreach (string email in AssignTo)
                {
                    EmailTo.Add(email);
                }
            }
            else
            {
                foreach (string email in To)
                {
                    EmailTo.Add(email);
                }
            }

            var currentUser = context.GetRequestUserVariable();
            var Description = context.ActivityBlueprint.DisplayName;

            Dictionary<string, string> listDynamicData = await _taskAppService.handleDynamicData(new TaskDynamicDataInput
            {
                WorkflowInstanceId = context.WorkflowInstance.Id,
            });

            if (listDynamicData.Count > 0)
            {
                foreach (var key in listDynamicData.Keys)
                {
                    this.Body = this.Body.Replace("{" + key + "}", listDynamicData[key]);
                }
            }

            var input = new AssignTaskInput
            {
                UserId = (Guid)currentUser.Id,
                WorkflowInstanceId = context.WorkflowInstance.Id,
                ApproveSignal = ApproveSignal.Trim(),
                RejectSignal = RejectSignal.Trim(),
                DynamicActionData = DynamicActionData,
                Description = Description,
                EmailTo = EmailTo,
                OtherActionSignals = OtherActionSignals
            };

            var taskId = await _taskAppService.assignTask(input, context.CancellationToken);
            this.Body = this.Body.Replace("${taskId}", taskId);
            if (DynamicActionData != null)
            {
                this.Body = this.Body.Replace("${input}", HttpUtility.UrlEncode(DynamicActionData) ?? "");
            }

            _taskQueue.EnqueueAsync(async (cancellationToken) => {
                await base.OnExecuteAsync(context);
            });

            return Done();
        }
    }
}
