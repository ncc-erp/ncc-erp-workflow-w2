using Elsa.Models;
using Elsa.Persistence;
using Elsa.Persistence.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using W2.Specifications;
using Microsoft.AspNetCore.Authorization;
using W2.ExternalResources;
using W2.Settings;
using W2.WorkflowDefinitions;

namespace W2.Mezon;

public class MezonAppService : W2AppService, IMezonAppService
{
    private readonly IWorkflowDefinitionStore _workflowDefinitionStore;
    private readonly IRepository<WorkflowCustomInputDefinition, Guid> _workflowCustomInputDefinitionRepository;
    private readonly IRepository<W2Setting, Guid> _settingRepository;
    private readonly IExternalResourceAppService _externalResourceAppService;

    public MezonAppService(
        IWorkflowDefinitionStore workflowDefinitionStore,
        IRepository<WorkflowCustomInputDefinition, Guid> workflowCustomInputDefinitionRepository,
        IRepository<W2Setting, Guid> settingRepository,
        IExternalResourceAppService externalResourceAppService
    )
    {
        _workflowDefinitionStore = workflowDefinitionStore;
        _workflowCustomInputDefinitionRepository = workflowCustomInputDefinitionRepository;
        _settingRepository = settingRepository;
        _externalResourceAppService = externalResourceAppService;
    }

    [AllowAnonymous]
    public async Task<MezonAppRequestTemplateDto> ListPropertyDefinitionsByCommand(ListPropertyDefinitionsByMezonCommandDto input)
    {
        var currentUserProjects = await GetCurrentUserProjectsAsync(input.Email);
        var listOffice = await GetListOfOffice();

        var specification = Specification<WorkflowDefinition>.Identity
            .And(new ListAllWorkflowDefinitionsSpecification(CurrentTenantStrId, null))
            .And(new PublishedWorkflowDefinitionsSpecification())
            .And(new GetByNameWorkflowDefinitionsSpecification(input.Keyword));

        var workflowDefinitionsFound = await _workflowDefinitionStore
            .FindManyAsync(
                specification,
                new OrderBy<WorkflowDefinition>(x => x.CreatedAt, SortDirection.Descending)
            );

        var workflowDefinition = workflowDefinitionsFound.FirstOrDefault();

        if (workflowDefinition == null)
        {
            throw new UserFriendlyException(L["Exception:WorkflowDefinitionNotFound"]);
        }

        var wfInputDefinition = await _workflowCustomInputDefinitionRepository
            .FindAsync(x => x.WorkflowDefinitionId == workflowDefinition.DefinitionId);

        var wfInputDefinitionDto =
            ObjectMapper.Map<WorkflowCustomInputDefinition, WorkflowCustomInputDefinitionMezonDto>(wfInputDefinition);
        var embed = new List<EmbedDto>();

        var selectTypes = new List<string> { "MyProject", "MyPMProject", "OfficeList" };
        foreach (var propertyDef in wfInputDefinitionDto.PropertyDefinitions)
        {
            var type = selectTypes.Contains(propertyDef.Type)
                ? MessageComponentTypeEnum.SELECT
                : MessageComponentTypeEnum.INPUT;

            var optionsMapping = new Dictionary<string, List<OptionDto>>
            {
                { "MyPMProject", currentUserProjects },
                { "MyProject", currentUserProjects },
                { "OfficeList", listOffice },
            };

            var options = optionsMapping.TryGetValue(propertyDef.Type, out var value)
                ? value
                : new List<OptionDto>();

            var component = new ComponentDto()
            {
                Id = propertyDef.Name,
                Type = MessageSelectTypeEnum.Text,
                Placeholder = propertyDef.Name,
                Required = propertyDef.IsRequired,
                Textarea = propertyDef.Type == "RichText",
                Options = options
            };

            var inputComponent = new InputDto()
            {
                Id = propertyDef.Name,
                Type = type,
                Component = component
            };

            var inputRef = new EmbedDto()
            {
                Name = propertyDef.Name,
                Value = "",
                Inputs = inputComponent
            };

            embed.Add(inputRef);
        }

        return new MezonAppRequestTemplateDto
        {
            WorkflowDefinitionId = workflowDefinition.DefinitionId,
            Embed = embed,
        };
    }

    private async Task<List<OptionDto>> GetListOfOffice()
    {
        var setting = await _settingRepository.FirstOrDefaultAsync(setting => setting.Code == SettingCodeEnum.DIRECTOR);
        var settingValue = setting.ValueObject;
        List<OptionDto> officeInfoList = new List<OptionDto>();
        settingValue.items.ForEach(item =>
        {
            officeInfoList.Add(new OptionDto
            {
                Value = item.code,
                Label = item.name,
            });
        });
        return officeInfoList;
    }

    private async Task<List<OptionDto>> GetCurrentUserProjectsAsync(string email)
    {
        if (email == null)
        {
            throw new UserFriendlyException(L["Exception:EmailNotFound"]);
        }

        var userEmail = CurrentUser.Email;
        if (!string.IsNullOrEmpty(email))
        {
            userEmail = email;
        }

        var projects = await _externalResourceAppService.GetUserProjectsFromApiAsync(userEmail);
        var listProjectsDto = new List<OptionDto>();
        foreach (var project in projects)
        {
            listProjectsDto.Add(new OptionDto
            {
                Value = project.Code,
                Label = project.Name,
            });
        }

        return listProjectsDto;
    }
}
    