﻿using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace W2.Mezon;

public interface IMezonAppService : IApplicationService
{
    Task<MezonAppRequestTemplateDto> ListPropertyDefinitionsByCommand(ListPropertyDefinitionsByMezonCommandDto input);
    Task<object> CreateNewInstanceAsync(CreateNewWorkflowInstanceDto input);
    Task<string> ApproveW2TaskAsync(ApproveTasksInput id, CancellationToken cancellationToken);
    Task<string> RejectW2TaskAsync(RejectTasksInput id, CancellationToken cancellationToken);
}