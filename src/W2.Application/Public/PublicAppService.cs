using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using W2.Authentication;
using W2.ExternalResources;
using W2.Tasks;

namespace W2.Public
{
    public class PublicAppService : W2AppService
    {
        private readonly IConfiguration _configuration;
        private readonly TaskAppService _taskAppService;
        private readonly ExternalResourceAppService _externalResourceAppService;

        public PublicAppService(
            IConfiguration configuration,
            TaskAppService taskAppService,
            ExternalResourceAppService externalResourceAppService
        ) {
            _configuration = configuration;
            _taskAppService = taskAppService;
            _externalResourceAppService = externalResourceAppService;
        }

        private async Task<bool> isValidMail(string email)
        {
            var mailDomain = "@" + _configuration["Authentication:Google:Domain"];  

            if (string.IsNullOrWhiteSpace(email) || !email.Contains(mailDomain))
            {
                throw new UserFriendlyException("Invalid Email!");
            }

            return true;
        }

        [HttpPost]
        [ExternalAuthentication]
        public async Task<PagedResultDto<W2TasksDto>> getListTasksByEmail(ListTasksInputExternal input)
        {
            await isValidMail(input.Email);

            return await _taskAppService.ListAsync(new ListTaskstInput
            {
                MaxResultCount = input.MaxResultCount,
                SkipCount = input.SkipCount,
                EmailAssignExternal = input.Email,
                EmailAssign = "",
                Status = input.Status,
                RequestName = input.RequestName,
                WorkflowDefinitionId = "",
                EmailRequest = "",
            });
        }

        [HttpPost]
        [ExternalAuthentication]
        public async Task<Dictionary<string, string>> ApproveTask(ApproveTasksInput input)
        {
            await isValidMail(input.Email);

            await _taskAppService.ApproveAsync(input);

            return new Dictionary<string, string> {
                { "id", input.Id.ToString() },
                { "message", "Approve Request Successfully!" }
            };
        }

        [HttpPost]
        [ExternalAuthentication]
        public async Task<Dictionary<string, string>> RejectTask(RejectTaskInput input)
        {
            await isValidMail(input.Email);

            await _taskAppService.RejectAsync(input.Id, input.Reason, input.Email);

            return new Dictionary<string, string> {
                { "id", input.Id.ToString() },
                { "message", "Reject Request Successfully!" }
            };
        }

        [HttpGet]
        [ExternalAuthentication]
        public async Task<TaskDetailDto> GetDetailByIdAsync(string id)
        {
            return await _taskAppService.GetDetailByIdAsync(id);
        }

        [HttpGet]
        [ExternalAuthentication]
        public async Task<List<OfficeInfo>> GetListOffices()
        {
            return await _externalResourceAppService.GetListOfOfficeAsync();
        }
    }
}
