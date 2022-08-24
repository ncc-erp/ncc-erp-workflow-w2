using Elsa.Activities.Http.Contracts;
using Elsa.Activities.Signaling.Extensions;
using Elsa.Scripting.JavaScript.Events;
using Elsa.Scripting.JavaScript.Messages;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Users;
using W2.ExternalResources;

namespace W2.Scripting
{
    public class CustomSignalJavaScriptHandler : INotificationHandler<EvaluatingJavaScriptExpression>, INotificationHandler<RenderingTypeScriptDefinitions>
    {
        private readonly IExternalResourceAppService _externalResourceAppService;
        private readonly ICurrentUser _currentUser;

        public CustomSignalJavaScriptHandler(IExternalResourceAppService externalResourceAppService, 
            ICurrentUser currentUser)
        {
            _externalResourceAppService = externalResourceAppService;
            _currentUser = currentUser;
        }

        public async Task Handle(EvaluatingJavaScriptExpression notification, CancellationToken cancellationToken)
        {
            var absoluteUrlProvider = notification.ActivityExecutionContext.GetService<IAbsoluteUrlProvider>();

            var engine = notification.Engine;
            engine.SetValue("workflowSignals", new WorkflowSignals());

            Func<string, string> getCustomSignalUrl = signal =>
            {
                var url = $"/Signals?token={notification.ActivityExecutionContext.GenerateSignalToken(signal)}";
                return absoluteUrlProvider.ToAbsoluteUrl(url).ToString();
            };
            engine.SetValue("getCustomSignalUrl", getCustomSignalUrl);

            var listOfOffices = await _externalResourceAppService.GetListOfOfficeAsync();
            Func<string, OfficeInfo> getOfficeInfo = officeCode =>
            {
                return listOfOffices.FirstOrDefault(x => x.Code == officeCode);
            };
            engine.SetValue("getOfficeInfo", getOfficeInfo);

            Func<ICurrentUser> getCurrentUser = () => _currentUser;
            engine.SetValue("getCurrentUser", getCurrentUser);
        }

        public Task Handle(RenderingTypeScriptDefinitions notification, CancellationToken cancellationToken)
        {
            var output = notification.Output;

            output.AppendLine("declare function getCustomSignalUrl(signal: string): string");
            output.AppendLine("declare const workflowSignals: WorkflowSignals");
            output.AppendLine("declare function getOfficeInfo(officeCode: string): OfficeInfo");
            output.AppendLine("declare function getCurrentUser(): ICurrentUser");

            return Task.CompletedTask;
        }
    }
}
