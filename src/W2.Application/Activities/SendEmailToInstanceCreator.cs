using Elsa;
using Elsa.Activities.Email.Options;
using Elsa.Activities.Email.Services;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Serialization;
using Elsa.Services.Models;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using W2.Localization;
using W2.WorkflowInstances;

namespace W2.Activities
{
    [Action(
        Category = "Email",
        DisplayName = "Send email to instance creator",
        Description = "Send an email to current instance creator.",
        Outcomes = new[] { OutcomeNames.Done, "Unexpected Error" })]
    public class SendEmailToInstanceCreator : CustomEmail
    {
        private readonly IRepository<WorkflowInstanceStarter, Guid> _workflowInstanceStarterRepository;
        private readonly IStringLocalizer<W2Resource> _localizer;
        private readonly IdentityUserManager _userManager;

        public SendEmailToInstanceCreator(ISmtpService smtpService,
            IOptions<SmtpOptions> options,
            IHttpClientFactory httpClientFactory,
            IContentSerializer contentSerializer,
            IRepository<WorkflowInstanceStarter, Guid> workflowInstanceStarterRepository,
            IStringLocalizer<W2Resource> localizer,
            IdentityUserManager userManager)
            : base(smtpService, options, httpClientFactory, contentSerializer)
        {
            _workflowInstanceStarterRepository = workflowInstanceStarterRepository;
            _localizer = localizer;
            _userManager = userManager;
        }

        public new ICollection<string> To { get; set; }
        public new ICollection<string> Cc { get; set; }
        public new ICollection<string> Bcc { get; set; }

        protected async override ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            var instanceStarter = await _workflowInstanceStarterRepository.FindAsync(x => x.WorkflowInstanceId == context.WorkflowInstance.Id);
            var outcomes = new List<string> { OutcomeNames.Done };
            if (instanceStarter == null)
            {
                outcomes.Add("Unexpected Error");
                context.JournalData.Add("Error", _localizer["Exception:InstanceNotFound"]);
                return Outcomes(outcomes);
            }

            try
            {
                var user = await _userManager.GetByIdAsync(instanceStarter.CreatorId.Value);
                base.To = new List<string> { user.Email };

                return await base.OnExecuteAsync(context);
            }
            catch (EntityNotFoundException ex)
            {
                outcomes.Add("Unexpected Error");
                context.JournalData.Add("Error", ex.Message);
                return Outcomes(outcomes);
            }
        }
    }
}
