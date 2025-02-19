using Elsa.Activities.Http.Contracts;
using Elsa.Activities.Signaling.Extensions;
using Elsa.Scripting.JavaScript.Events;
using Elsa.Scripting.JavaScript.Messages;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
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
        private readonly IConfiguration _configuration;

        public CustomSignalJavaScriptHandler(
            IExternalResourceAppService externalResourceAppService,
            ICurrentUser currentUser,
            IConfiguration configuration)
        {
            _externalResourceAppService = externalResourceAppService;
            _currentUser = currentUser;
            _configuration = configuration;
        }

        public async Task Handle(EvaluatingJavaScriptExpression notification, CancellationToken cancellationToken)
        {
            var absoluteUrlProvider = notification.ActivityExecutionContext.GetService<IAbsoluteUrlProvider>();

            if(_configuration.GetValue<string>("URLWeb") == "")
            {
                throw new UserFriendlyException("Exception:URL Web not exist");
            }
            string UrlTask = _configuration.GetValue<string>("URLWeb") + "/tasks?id=${taskId}&action=";
            string UrlApproveTask = UrlTask + "approve&input=${input}";
            string UrlRejectTask = UrlTask + "reject";

            var engine = notification.Engine;
            engine.SetValue("workflowSignals", new WorkflowSignals());

            Func<string, string> getCustomSignalUrl = signal =>
            {
                var url = $"/Signals?token={notification.ActivityExecutionContext.GenerateSignalToken(signal)}";
                return absoluteUrlProvider.ToAbsoluteUrl(UrlApproveTask).ToString();
            };
            engine.SetValue("getCustomSignalUrl", getCustomSignalUrl);

            Func<string, string> getOtherActionSignalUrl = signal =>
            {
                string UrlActionTask = UrlTask + "other&input=" + signal;
                var url = $"/Signals?token={notification.ActivityExecutionContext.GenerateSignalToken(signal)}";
                return absoluteUrlProvider.ToAbsoluteUrl(UrlActionTask).ToString();
            };
            engine.SetValue("getOtherActionSignalUrl", getOtherActionSignalUrl);

            Func<string, string[], string> getCustomSignalUrlWithForm = (signal, requiredInputs) =>
            {
                var url = $"/Signals/Form?token={notification.ActivityExecutionContext.GenerateSignalTokenWithForm(signal, requiredInputs)}";
                return absoluteUrlProvider.ToAbsoluteUrl(UrlRejectTask).ToString();
            };
            engine.SetValue("getCustomSignalUrlWithForm", getCustomSignalUrlWithForm);

            var listOfOffices = await _externalResourceAppService.GetListOfOfficeAsync();

            var listOfProjects = await _externalResourceAppService.GetUserProjectsFromApiAsync(notification.ActivityExecutionContext.GetRequestUserVariable()?.Email);

            if (!string.IsNullOrEmpty(notification.ActivityExecutionContext.GetRequestUserVariable()?.TargetStaffEmail))
            {
                listOfProjects = await _externalResourceAppService.GetUserProjectsFromApiAsync(notification.ActivityExecutionContext.GetRequestUserVariable()?.TargetStaffEmail);
            }

            Func<string, OfficeInfo> getOfficeInfo = officeCode =>
            {
                return listOfOffices.FirstOrDefault(x => x.Code == officeCode);
            };

            Func<string, TimesheetProjectItem> getProjectInfo = projectCode =>
            {
                //var branchResult = await _externalResourceAppService.GetUserBranchInfoAsync(_currentUser.Email);
                var project = listOfProjects.FirstOrDefault(x => x.Code.ToLower() == projectCode.ToLower());
                return project ?? throw new Exception("User's not in any project");
                //return project
                //       ?? new TimesheetProjectItem
                //       {
                //           Code = "Default",
                //           Name = "Default",
                //           PM = new List<ProjectManager>
                //           {
                //               new ProjectManager
                //               {
                //                   FullName = branchResult.HeadOfOfficeEmail?.Split('@')[0],
                //                   EmailAddress = branchResult.HeadOfOfficeEmail,
                //               }
                //           }
                //       };
            };

            engine.SetValue("getProjectInfo", getProjectInfo);

            engine.SetValue("getOfficeInfo", getOfficeInfo);

            engine.SetValue("currentUser", _currentUser);

            engine.SetValue("currentUserProject", _currentUser.FindClaimValue(CustomClaim.ProjectName));

            engine.SetValue("signalInputTypes", new SignalInputTypes());

            Func<RequestUser> getRequestUser = () => notification.ActivityExecutionContext.GetRequestUserVariable();
            engine.SetValue("getRequestUser", getRequestUser);

            engine.SetValue("workflowInstanceVariableNames", new WorkflowInstanceVariableNames());
        }

        public Task Handle(RenderingTypeScriptDefinitions notification, CancellationToken cancellationToken)
        {
            var output = notification.Output;

            output.AppendLine("declare function getCustomSignalUrl(signal: string): string");
            output.AppendLine("declare function getOtherActionSignalUrl(signal: string): string");
            output.AppendLine("declare const workflowSignals: WorkflowSignals");
            output.AppendLine("declare function getOfficeInfo(officeCode: string): OfficeInfo");
            output.AppendLine("declare const currentUser: ICurrentUser");
            output.AppendLine("declare const currentUserProject: string");
            output.AppendLine("declare function getCustomSignalUrlWithForm(signal: string, requiredInputs: string[]): string");
            output.AppendLine("declare const signalInputTypes: SignalInputTypes");
            output.AppendLine("declare function getRequestUser: RequestUser");
            output.AppendLine("declare const workflowInstanceVariableNames: WorkflowInstanceVariableNames");

            return Task.CompletedTask;
        }
    }
}
