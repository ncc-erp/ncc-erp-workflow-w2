using Elsa.Activities.Http.Contracts;
using Elsa.Activities.Signaling.Extensions;
using Elsa.Scripting.JavaScript.Events;
using Elsa.Scripting.JavaScript.Messages;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace W2.Scripting
{
    public class CustomSignalJavaScriptHandler : INotificationHandler<EvaluatingJavaScriptExpression>, INotificationHandler<RenderingTypeScriptDefinitions>
    {
        public Task Handle(EvaluatingJavaScriptExpression notification, CancellationToken cancellationToken)
        {
            var absoluteUrlProvider = notification.ActivityExecutionContext.GetService<IAbsoluteUrlProvider>();

            var engine = notification.Engine;
            engine.SetValue("workflowSignals", new WorkflowSignals());
            Func<string, string> getCustomSignalUrl = signal => {
                var url = $"/Signals?token={notification.ActivityExecutionContext.GenerateSignalToken(signal)}";
                return absoluteUrlProvider.ToAbsoluteUrl(url).ToString();
            };
            engine.SetValue("getCustomSignalUrl", getCustomSignalUrl);

            return Task.CompletedTask;
        }

        public Task Handle(RenderingTypeScriptDefinitions notification, CancellationToken cancellationToken)
        {
            var output = notification.Output;

            output.AppendLine("declare function getCustomSignalUrl(signal: string): string");
            output.AppendLine("declare const workflowSignals: WorkflowSignals");

            return Task.CompletedTask;
        }
    }
}
