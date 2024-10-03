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
using W2.Scripting;
using W2.Tasks;

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
        public CustomEmail(ISmtpService smtpService,
            IOptions<SmtpOptions> options,
            IHttpClientFactory httpClientFactory,
            ITaskAppService taskAppService,
            IContentSerializer contentSerializer)
            : base(smtpService, options, httpClientFactory, contentSerializer)
        {
            _taskAppService = taskAppService;
        }

        public new string From => string.Empty;

        protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            List<DynamicDataDto> dynamicDataByTask = await _taskAppService.GetDynamicRawData(new TaskDynamicDataInput
            {
                WorkflowInstanceId = context.WorkflowInstance.Id,
            });

            if (dynamicDataByTask != null) { 
                context.SetVariable("DynamicDataByTask", dynamicDataByTask); 
            }

            _ = Task.Run(async () =>
            {
                await base.OnExecuteAsync(context);
            });

            return Done();
        }
    }
}
