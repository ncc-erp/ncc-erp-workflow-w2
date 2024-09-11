using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Services;
using Elsa.Services.Models;
using Humanizer;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Users;
using W2.ExternalResources;
using W2.Permissions;
using W2.Scripting;
using Volo.Abp.Domain.Repositories;
using W2.Settings;
using Volo.Abp.SettingManagement;
using Newtonsoft.Json;

namespace W2.Activities
{
    [Activity(
        DisplayName = "Set RequestUser Variable", 
        Description = "Set RequestUser variable on the workflow.", 
        Category = "Primitives", 
        Outcomes = new string[] { "Done" })]
    public class SetRequestUserVariable : Activity
    {
        private readonly ICurrentUser _currentUser;
        private readonly IProjectClientApi _projectClientApi;
        private readonly IExternalResourceAppService _externalResourceAppService;
        private readonly IRepository<W2Setting, Guid> _settingRepository;

        public SetRequestUserVariable(ICurrentUser currentUser,
            IProjectClientApi projectClientApi,
            IExternalResourceAppService externalResourceAppService,
            IRepository<W2Setting, Guid> settingRepository)
        {
            _currentUser = currentUser;
            _projectClientApi = projectClientApi;
            _externalResourceAppService = externalResourceAppService;
            _settingRepository = settingRepository;
        }

        protected async override ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            string targetStaffEmail = null;

            try
            {
                // Check if context and context.Input are not null
                if (context != null && context.Input != null)
                {
                    var bodyProperty = context.Input.GetType().GetProperty("Body");

                    // Check if the 'Body' property exists and is not null
                    if (bodyProperty != null)
                    {
                        var instanceInput = bodyProperty.GetValue(context.Input);

                        if (instanceInput is IDictionary<string, string> valueDictionary && valueDictionary.ContainsKey("Staff"))
                        {
                            targetStaffEmail = valueDictionary["Staff"];
                        }
                    }
                }
            }
            catch (Exception)
            {
                targetStaffEmail = null;
            }

            // set project
            // set branch info
            var branchResult = await _externalResourceAppService.GetUserBranchInfoAsync(_currentUser.Email);
            //To.Add(branchResult.HeadOfOfficeEmail);
            // set PM
            var userProjectsResult = await _projectClientApi.GetUserProjectsAsync(_currentUser.Email);
            ProjectProjectItem project = null;
            if (userProjectsResult?.Result != null && userProjectsResult?.Result.Count > 0)
            {
                project = userProjectsResult.Result.First();
            }

            var w2Setting = await _settingRepository.GetListAsync();
            var ITEmails = new List<string>();
            var CEOEmails = new List<string>();
            var GDVPEmails = new List<string>();
            var HREmails = new List<string>();
            var SaleEmails = new List<string>();
            w2Setting.ForEach(setting => {
                var settingValue = JsonConvert.DeserializeObject<W2SettingValue>(setting.Value);
                switch (setting.Code)
                {
                    case "IT":
                        settingValue.items.ForEach(item => ITEmails.Add(item.email));
                        break;
                    case "CEO":
                        settingValue.items.ForEach(item => CEOEmails.Add(item.email));
                        break;
                    case "GDVP":
                        settingValue.items.ForEach(item => GDVPEmails.Add(item.email));
                        break;
                    case "HR":
                        settingValue.items.ForEach(item => HREmails.Add(item.email));
                        break;
                    case "Sale":
                        settingValue.items.ForEach(item => SaleEmails.Add(item.email));
                        break;
                }
            });

            var requestUser = new RequestUser
            {
                Id = _currentUser.Id,
                Email = _currentUser.Email,
                Name = _currentUser.Name,
                TargetStaffEmail = targetStaffEmail,
                Project = _currentUser.FindClaimValue(CustomClaim.ProjectName),
                HeadOfOfficeEmail = branchResult?.HeadOfOfficeEmail,
                BranchCode = branchResult?.Code,
                ITEmails = ITEmails,
                CEOEmails = CEOEmails,
                GDVPEmails = GDVPEmails,
                HREmails = HREmails,
                SaleEmails = SaleEmails,
                BranchName = branchResult?.DisplayName,
                ProjectCode = project?.Code,
                PM = project?.PM?.EmailAddress
            };
            context.SetVariable(nameof(RequestUser), requestUser);
            return Done();
        }
    }
}
