using Elsa;
using Elsa.Activities.Email.Options;
using Elsa.Activities.Email.Services;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Serialization;
using Elsa.Services.Models;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using W2.ExternalResources;
using W2.Localization;
using W2.Signals;
using W2.Tasks;
using W2.WorkflowInstances;

namespace W2.Activities
{
    [Action(
        Category = "Email",
        DisplayName = "Send email and Assign Task",
        Description = "Send an email and assign an task to the PM",
        Outcomes = new[] { OutcomeNames.Done, "Unexpected Error" })]
    public class SendMailAndAsssign : CustomEmail
    {
        private readonly IRepository<MyTask, Guid> _myTaskRepository;
        private readonly IExternalResourceAppService _externalResourceAppService;
        public SendMailAndAsssign(ISmtpService smtpService,
            IOptions<SmtpOptions> options,
            IHttpClientFactory httpClientFactory,
            IContentSerializer contentSerializer,
            IRepository<MyTask, Guid> myTaskRepository,
            IExternalResourceAppService externalResourceAppService)
            : base(smtpService, options, httpClientFactory, contentSerializer)
        {
            _myTaskRepository = myTaskRepository;
            _externalResourceAppService = externalResourceAppService;
        }

        public new ICollection<string> Cc { get; set; }
        public new ICollection<string> Bcc { get; set; }

        protected async override ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            var outcomes = new List<string> { OutcomeNames.Done };

            try
            {
                var requestUser = context.GetRequestUserVariable();
                /*
                if (base.To == null)
                {
                    base.To = new List<string>();
                }

                if (requestUser != null && !To.Contains(requestUser.Email))
                {
                    // send mail to creator
                    base.To.Add(requestUser.Email);

                    // send mail to PM
                    base.To.Add(requestUser?.PM);

                    // send mail to head office
                    var userProjectsResult = await _externalResourceAppService.GetUserBranchInfoAsync(requestUser.Email);
                    To.Add(userProjectsResult.HeadOfOfficeEmail);
                }

                // only send mail to creator yet
                string id = context.WorkflowExecutionContext.WorkflowInstance.Id;
                var newTask = new MyTask { Email = requestUser?.PM, Status = requestUser?.ProjectCode, WorkflowInstanceId = id };
                await _myTaskRepository.InsertAsync(newTask);

                return await base.OnExecuteAsync(context);
                */

                Console.WriteLine("TOOOOOOOO");
                Console.WriteLine(JsonConvert.SerializeObject(To));

                string id = context.WorkflowExecutionContext.WorkflowInstance.Id;
                foreach (string item in To)
                {
                    var newTask = new MyTask { Email = item, Status = requestUser?.ProjectCode, WorkflowInstanceId = id };
                    await _myTaskRepository.InsertAsync(newTask);
                }
  
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
