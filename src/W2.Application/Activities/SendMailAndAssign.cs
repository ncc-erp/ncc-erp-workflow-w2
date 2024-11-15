using Elsa;
using Elsa.Activities.Email;
using Elsa.Activities.Email.Options;
using Elsa.Activities.Email.Services;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Design;
using Elsa.Expressions;
using Elsa.Serialization;
using Elsa.Services.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Volo.Abp;
using W2.Komu;
using W2.Scripting;
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
        private IKomuService _komuService;
        public SendMailAndAssign(ISmtpService smtpService, 
            IOptions<SmtpOptions> options, 
            IHttpClientFactory httpClientFactory,
            ITaskAppService taskAppService,
            IKomuService komuService,
            IContentSerializer contentSerializer) 
            : base(smtpService, options, httpClientFactory, contentSerializer)
        {
            _taskAppService = taskAppService;
            _komuService = komuService;
        }

        public new string From => string.Empty;

        [ActivityInput(Hint = "The approved signal", SupportedSyntaxes = new string[] { "JavaScript", "Liquid" })]
        public string ApproveSignal { get; set; }

        [ActivityInput(Hint = "The reject signal", SupportedSyntaxes = new string[] { "JavaScript", "Liquid" })]
        public string RejectSignal { get; set; }

        [ActivityInput(Hint = "The dynamic form data", SupportedSyntaxes = new string[] { "JavaScript", "Liquid" })]
        public string DynamicActionData { get; set; }

        [ActivityInput(Label = "Komu message", Hint = "The message that you want to send by KOMU to request users", UIHint = ActivityInputUIHints.MultiLine, DefaultSyntax = SyntaxNames.Literal, SupportedSyntaxes = new string[] { SyntaxNames.JavaScript, SyntaxNames.Literal })]
        public string KomuMessage { get; set; }

        [ActivityInput(Hint = "Other action for signal", UIHint = "multi-text", DefaultSyntax = "Json", SupportedSyntaxes = new string[] { "Json", "JavaScript" })]
        public List<string> OtherActionSignals { get; set; }

        [ActivityInput(Hint = "Email to assign", UIHint = "multi-text", DefaultSyntax = "Json", SupportedSyntaxes = new string[] { "Json", "JavaScript" })]
        public List<string> AssignTo { get; set; }

        protected async override ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            List<DynamicDataDto> dynamicDataByTask = await _taskAppService.GetDynamicRawData(new TaskDynamicDataInput
            {
                WorkflowInstanceId = context.WorkflowInstance.Id,
            });

            if (dynamicDataByTask != null)
            {
                context.SetVariable("DynamicDataByTask", dynamicDataByTask);
            }

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

                    if(this.KomuMessage != null)
                    {
                        string transString = listDynamicData[key].Replace("</p><p>", "\n");
                        transString = Regex.Replace(transString, @"<\/?p>", "");
                        this.KomuMessage = this.KomuMessage?.Replace("{" + key + "}", transString);
                    }
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

            _ = Task.Run(async () =>
            {
                await base.OnExecuteAsync(context);
            });

            foreach (var email in EmailTo)
            {
                _ = Task.Run(async () =>
                {
                    var emailPrefix = email?.Split('@')[0];
                    await _komuService.KomuSendMessageAsync(emailPrefix, KomuMessage);
                });
            }

            return Done();
        }
    }
}
