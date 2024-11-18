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
using System.Threading.Tasks;
using W2.Komu;
using W2.Scripting;
using W2.Tasks;
using W2.WorkflowDefinitions;

namespace W2.Activities
{
    [Action(
        Category = "Email",
        DisplayName = "Default Sender Email",
        Description = "Send an email message with default sender.",
        Outcomes = new[] { OutcomeNames.Done, "Unexpected Error" })]
    public class CustomEmail : SendEmail
    {
        private readonly ITaskAppService _taskAppService;
        private IKomuService _komuService;
        private IWorkflowDefinitionAppService _workflowDefinitionAppService;
        public CustomEmail(ISmtpService smtpService,
            IOptions<SmtpOptions> options,
            IHttpClientFactory httpClientFactory,
            ITaskAppService taskAppService,
            IKomuService komuService,
            IWorkflowDefinitionAppService workflowDefinitionAppService,
            IContentSerializer contentSerializer)
            : base(smtpService, options, httpClientFactory, contentSerializer)
        {
            _taskAppService = taskAppService;
            _komuService = komuService;
            _workflowDefinitionAppService = workflowDefinitionAppService;
        }

        public new string From => string.Empty;

        [ActivityInput(Label = "Komu message", Hint = "The message that you want to send by KOMU to request users", UIHint = ActivityInputUIHints.MultiLine, DefaultSyntax = SyntaxNames.Literal, SupportedSyntaxes = new string[] { SyntaxNames.JavaScript, SyntaxNames.Literal })]
        public string KomuMessage { get; set; }

        protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            List<DynamicDataDto> dynamicDataByTask = await _taskAppService.GetDynamicRawData(new TaskDynamicDataInput
            {
                WorkflowInstanceId = context.WorkflowInstance.Id,
            });

            if (dynamicDataByTask != null) { 
                context.SetVariable("DynamicDataByTask", dynamicDataByTask); 
            }

            WorkflowDefinitionSummaryDto workflowDefinitionSummaryDto = await _workflowDefinitionAppService.GetByDefinitionIdAsync(context.WorkflowInstance.DefinitionId);

            _ = Task.Run(async () =>
            {
                await base.OnExecuteAsync(context);
            });

            if ((bool)workflowDefinitionSummaryDto?.InputDefinition.Settings.IsSendKomuMessage)
            {
                foreach (var email in this.To)
                {
                    _ = Task.Run(async () =>
                    {
                        var emailPrefix = email?.Split('@')[0];
                        await _komuService.KomuSendMessageAsync(emailPrefix, KomuMessage);
                    });
                }
            }

            return Done();
        }
    }
}
