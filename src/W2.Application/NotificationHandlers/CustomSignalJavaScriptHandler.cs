using Elsa.Activities.Http.Contracts;
using Elsa.Activities.Signaling.Extensions;
using Elsa.Scripting.JavaScript.Messages;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace W2.NotificationHandlers
{
    public class CustomSignalJavaScriptHandler : INotificationHandler<EvaluatingJavaScriptExpression>
    {
        public Task Handle(EvaluatingJavaScriptExpression notification, CancellationToken cancellationToken)
        {
            var absoluteUrlProvider = notification.ActivityExecutionContext.GetService<IAbsoluteUrlProvider>();

            var engine = notification.Engine;
            engine.SetValue("workflowSignals", new
            {
                W2Consts.WorkflowSignals.PMApproved,
                W2Consts.WorkflowSignals.PMRejected,
                W2Consts.WorkflowSignals.HoOApproved,
                W2Consts.WorkflowSignals.HoORejected,
            });
            Func<string, string> getCustomSignalUrl = signal => {
                var url = $"/Signals?token={notification.ActivityExecutionContext.GenerateSignalToken(signal)}";
                return absoluteUrlProvider.ToAbsoluteUrl(url).ToString();
            };
            engine.SetValue("getCustomSignalUrl", getCustomSignalUrl);

            return Task.CompletedTask;
        }
    }
}
