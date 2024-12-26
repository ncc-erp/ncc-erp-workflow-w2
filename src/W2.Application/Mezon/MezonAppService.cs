using Elsa.Models;
using Elsa.Persistence;
using Elsa.Persistence.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Elsa.Services;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using W2.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Volo.Abp.Security.Claims;
using Volo.Abp.Uow;
using W2.ExternalResources;
using W2.Identity;
using W2.Komu;
using W2.Settings;
using W2.Utils;
using W2.WorkflowDefinitions;
using W2.WorkflowInstances;

namespace W2.Mezon;

public class MezonAppService : W2AppService, IMezonAppService
{
    private readonly IWorkflowDefinitionStore _workflowDefinitionStore;
    private readonly IRepository<WorkflowCustomInputDefinition, Guid> _workflowCustomInputDefinitionRepository;
    private readonly IRepository<W2Setting, Guid> _settingRepository;
    private readonly IExternalResourceAppService _externalResourceAppService;
    private readonly IWorkflowLaunchpad _workflowLaunchpad;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IRepository<WorkflowInstanceStarter, Guid> _instanceStarterRepository;
    private readonly IRepository<W2CustomIdentityUser, Guid> _userRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IKomuAppService _komuAppService;

    public MezonAppService(
        IRepository<WorkflowInstanceStarter, Guid> instanceStarterRepository,
        IWorkflowLaunchpad workflowLaunchpad,
        IUnitOfWorkManager unitOfWorkManager,
        IWorkflowDefinitionStore workflowDefinitionStore,
        IRepository<WorkflowCustomInputDefinition, Guid> workflowCustomInputDefinitionRepository,
        IRepository<W2Setting, Guid> settingRepository,
        IExternalResourceAppService externalResourceAppService,
        IRepository<W2CustomIdentityUser, Guid> userRepository,
        IHttpContextAccessor httpContextAccessor,
        IKomuAppService komuAppService
    )
    {
        _workflowDefinitionStore = workflowDefinitionStore;
        _workflowCustomInputDefinitionRepository = workflowCustomInputDefinitionRepository;
        _settingRepository = settingRepository;
        _externalResourceAppService = externalResourceAppService;
        _workflowLaunchpad = workflowLaunchpad;
        _unitOfWorkManager = unitOfWorkManager;
        _instanceStarterRepository = instanceStarterRepository;
        _userRepository = userRepository;
        _httpContextAccessor = httpContextAccessor;
        _komuAppService = komuAppService;
    }

    [AllowAnonymous]
    [ApiKeyAuth]
    public async Task<MezonAppRequestTemplateDto> ListPropertyDefinitionsByCommand(
        ListPropertyDefinitionsByMezonCommandDto input)
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
        var dateTimeTypes = new List<string> { "MultiDatetime", "DateTime"};
        foreach (var propertyDef in wfInputDefinitionDto.PropertyDefinitions)
        {
            var type = selectTypes.Contains(propertyDef.Type)
                ? MessageComponentTypeEnum.SELECT
                : MessageComponentTypeEnum.INPUT;

            var name = dateTimeTypes.Contains(propertyDef.Type)
                ? $"{propertyDef.Name} (dd/MM/yyyy)"
                : propertyDef.Name;

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
                Name = name,
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

        var projects = await _externalResourceAppService.GetUserProjectsFromApiAsync(email);
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

    [AllowAnonymous]
    [ApiKeyAuth]
    public async Task<object> CreateNewInstanceAsync(CreateNewWorkflowInstanceDto input)
    {
        await UpdateCurrentUser(input.Email);

        var startableWorkflow = await _workflowLaunchpad.FindStartableWorkflowAsync(
            input.WorkflowDefinitionId,
            tenantId: CurrentTenantStrId
        );

        if (startableWorkflow == null)
        {
            throw new UserFriendlyException(L["Exception:NoStartableWorkflowFound"]);
        }

        var httpRequestModel = GetHttpRequestModel(nameof(HttpMethod.Post), input.DataInputs);

        var executionResult = await _workflowLaunchpad.ExecuteStartableWorkflowAsync(
            startableWorkflow,
            new WorkflowInput(httpRequestModel)
        );

        var instance = executionResult.WorkflowInstance;
        var workflowInstanceStarterResponse = new WorkflowInstanceStarter();
        using (var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false))
        {
            var workflowInstanceStarter = new WorkflowInstanceStarter
            {
                WorkflowInstanceId = instance.Id,
                WorkflowDefinitionId = instance.DefinitionId,
                WorkflowDefinitionVersionId = instance.DefinitionVersionId,
                Input = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                    JsonConvert.SerializeObject(input.DataInputs)),
            };

            workflowInstanceStarterResponse = await _instanceStarterRepository.InsertAsync(workflowInstanceStarter);
            await uow.CompleteAsync();
        }

        await _komuAppService.KomuSendTaskAssignAsync("khanh.tranvan", (Guid)CurrentUser.Id, instance.Id);

        _httpContextAccessor.HttpContext.User = new ClaimsPrincipal();
        return workflowInstanceStarterResponse;
    }

    private async Task<W2CustomIdentityUser> UpdateCurrentUser(string email)
    {
        var userByEmail = await _userRepository.FirstOrDefaultAsync(u => u.Email == email);
        if (userByEmail == null)
        {
            throw new UserFriendlyException(L["Exception:EmailNotFound"]);
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            throw new Exception("HttpContext is null. Ensure this method is called in a valid HTTP request context.");
        }

        var claims = new List<Claim>
        {
            new Claim(AbpClaimTypes.UserId, userByEmail.Id.ToString()),
            new Claim(AbpClaimTypes.UserName, userByEmail.UserName),
            new Claim(AbpClaimTypes.Email, userByEmail.Email),
            new Claim(AbpClaimTypes.Name, userByEmail.Name),
        };

        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        httpContext.User = principal;

        return userByEmail;
    }
}