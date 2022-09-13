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
using W2.Permissions;
using W2.Signals;

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

            Func<string, string[], string> getCustomSignalUrlWithForm = (signal, requiredInputs) =>
            {
                var url = $"/Signals/Form?token={notification.ActivityExecutionContext.GenerateSignalTokenWithForm(signal, requiredInputs)}";
                return absoluteUrlProvider.ToAbsoluteUrl(url).ToString();
            };
            engine.SetValue("getCustomSignalUrlWithForm", getCustomSignalUrlWithForm);

            var listOfOffices = await _externalResourceAppService.GetListOfOfficeAsync();
            Func<string, OfficeInfo> getOfficeInfo = officeCode =>
            {
                return listOfOffices.FirstOrDefault(x => x.Code == officeCode);
            };
            engine.SetValue("getOfficeInfo", getOfficeInfo);

            engine.SetValue("currentUser", _currentUser);

            engine.SetValue("currentUserProject", _currentUser.FindClaimValue(CustomClaim.ProjectName));

            engine.SetValue("signalInputTypes", new SignalInputTypes());
        }

        public Task Handle(RenderingTypeScriptDefinitions notification, CancellationToken cancellationToken)
        {
            var output = notification.Output;

            output.AppendLine("declare function getCustomSignalUrl(signal: string): string");
            output.AppendLine("declare const workflowSignals: WorkflowSignals");
            output.AppendLine("declare function getOfficeInfo(officeCode: string): OfficeInfo");
            output.AppendLine("declare const currentUser: ICurrentUser");
            output.AppendLine("declare const currentUserProject: string");
            output.AppendLine("declare function getCustomSignalUrlWithForm(signal: string, requiredInputs: string[]): string");
            output.AppendLine("declare const signalInputTypes: SignalInputTypes");

            return Task.CompletedTask;
        }
    }
}
