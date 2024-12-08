using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using W2.WorkflowDefinitions;

namespace W2.Mezon;

public interface IMezonAppService : IApplicationService
{
    Task<MezonAppRequestTemplateDto> ListPropertyDefinitionsByCommand(ListPropertyDefinitionsByMezonCommandDto input);
}