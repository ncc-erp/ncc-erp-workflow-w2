using Elsa.Models;
using Elsa.Persistence;
using Elsa.Persistence.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Elsa.Activities.Signaling.Services;
using Elsa.Services;
using IdentityModel;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using W2.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Security.Claims;
using Volo.Abp.Uow;
using Volo.Abp.Users;
using W2.ExternalResources;
using W2.Identity;
using W2.Komu;
using W2.Permissions;
using W2.Settings;
using W2.TaskEmail;
using W2.Tasks;
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
    private readonly IRepository<W2Task, Guid> _taskRepository;
    private readonly IRepository<W2TaskEmail, Guid> _taskEmailRepository;
    private readonly ISignaler _signaler;
    private readonly ICurrentUser _currentUser;

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
        IKomuAppService komuAppService,
        IRepository<W2Task, Guid>taskRepository,
        IRepository<W2TaskEmail, Guid> taskEmailRepository,
        ISignaler signaler,
        ICurrentUser currentUser
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
        _taskRepository = taskRepository;
        _signaler = signaler;
        _taskEmailRepository = taskEmailRepository;
        _currentUser = currentUser;
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

        await _komuAppService.KomuSendTaskAssignAsync((Guid)CurrentUser.Id, instance.Id);

        _httpContextAccessor.HttpContext.User = new ClaimsPrincipal();
        return workflowInstanceStarterResponse;
    }

    private async Task<W2CustomIdentityUser> UpdateCurrentUser(string email)
    {
        var query = await _userRepository.GetQueryableAsync();
        var userByEmail = await query
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Where(u => u.Email == email)
            .FirstOrDefaultAsync();

        if (userByEmail == null)
        {
            throw new UserFriendlyException(L["Exception:EmailNotFound"]);
        }

        var roleNames = userByEmail.UserRoles
            .Select(ur => ur.Role.Name)
            .ToArray();
        var rolePermissions = userByEmail.UserRoles
            .SelectMany(ur => ur.Role.PermissionDtos)
            .ToList();
        var rolePermissionCodes = W2Permission.GetPermissionCodes(rolePermissions);
        var customPermissionCodes = W2Permission.GetPermissionCodes(userByEmail.CustomPermissionDtos);
        var allPermissionCodes = rolePermissionCodes
            .Union(customPermissionCodes)
            .OrderBy(x => x)
            .ToList();

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
        claims.AddRange(roleNames.Select(
            role => new Claim(JwtClaimTypes.Role, role))
        );
        claims.AddRange(allPermissionCodes.Select(
            permission => new Claim("permissions", permission))
        );

        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        httpContext.User = principal;

        return userByEmail;
    }

    [AllowAnonymous]
    [ApiKeyAuth]
    public async Task<string> ApproveW2TaskAsync(ApproveTasksInput input, CancellationToken cancellationToken)
    {
        await UpdateCurrentUser(input.Email);
        var myTask = await _taskRepository.FirstOrDefaultAsync(x => x.Id == Guid.Parse(input.Id));
        if (myTask == null)
        {
            throw new EntityNotFoundException(L["Exception:TaskNotFound"]);
        }
        if (myTask.Status != W2TaskStatus.Pending)
        {
            if (myTask.Status == W2TaskStatus.Approve)
            { 
                throw new UserFriendlyException(L["Exception:MyTaskHasBeenApproved"], ((int)HttpStatusCode.Conflict).ToString());
            }
            else
            {
                throw new UserFriendlyException(L["Exception:MyTaskHasBeenRejected"], ((int)HttpStatusCode.Conflict).ToString());
            }
        }
        
        var taskEmail = (await _taskEmailRepository.GetListAsync(x => x.TaskId == myTask.Id.ToString()))
            .Where(x => x.Email == input.Email && x.TaskId == myTask.Id.ToString())
            .ToList().FirstOrDefault();
        
        if (taskEmail == null)
        {
            throw new UserFriendlyException(L["Exception:No Permission"]);
        }
        
        var Inputs = new Dictionary<string, string>
        {
            { "Reason", $"{myTask.ApproveSignal}" },
            { "TriggeredBy", $"{input.Email}" },
            { "TriggeredByName", $"{_currentUser.Name}" }
        };
        
        if (!string.IsNullOrEmpty(input.DynamicActionData))
        {
            myTask.DynamicActionData = input.DynamicActionData;
        }
        
        myTask.Status = W2TaskStatus.Approve;
        myTask.UpdatedBy = input.Email;
        await _taskRepository.UpdateAsync(myTask, true);// avoid conflict with approve signal
        
        await _signaler.TriggerSignalAsync(myTask.ApproveSignal, Inputs, myTask.WorkflowInstanceId, cancellationToken: cancellationToken);

        return "Approval successful";
    }
    
    [AllowAnonymous]
    [ApiKeyAuth]
    public async Task<string> RejectW2TaskAsync(RejectTasksInput input, CancellationToken cancellationToken)
    {
        await UpdateCurrentUser(input.Email);
        var myTask = await _taskRepository.FirstOrDefaultAsync(x => x.Id == Guid.Parse(input.Id));
        if (myTask == null)
        {
            throw new EntityNotFoundException(L["Exception:TaskNotFound"]);
        }
        if (myTask.Status != W2TaskStatus.Pending)
        {
            if (myTask.Status == W2TaskStatus.Approve)
            { 
                throw new UserFriendlyException(L["Exception:MyTaskHasBeenApproved"], ((int)HttpStatusCode.Conflict).ToString());
            }
            else
            {
                throw new UserFriendlyException(L["Exception:MyTaskHasBeenRejected"], ((int)HttpStatusCode.Conflict).ToString());
            }
        }

        var taskEmail = (await _taskEmailRepository.GetListAsync(x => x.TaskId == myTask.Id.ToString()))
            .Where(x => x.Email == _currentUser.Email && x.TaskId == myTask.Id.ToString())
            .ToList().FirstOrDefault();

        if (taskEmail == null)
        {
            throw new UserFriendlyException(L["Exception:No Permission"]);
        }

        var Inputs = new Dictionary<string, string>
        {
            { "Reason", $"{input.Reason}" },
            { "TriggeredBy", $"{_currentUser.Email}" },
            { "TriggeredByName", $"{_currentUser.Name}" }
        };
        myTask.Status = W2TaskStatus.Reject;
        myTask.UpdatedBy = _currentUser.Email;
        myTask.Reason = input.Reason;
        await _taskRepository.UpdateAsync(myTask, true);// avoid conflict with approve signal

        await _signaler.TriggerSignalAsync(myTask.RejectSignal, Inputs, myTask.WorkflowInstanceId, cancellationToken: cancellationToken);

        return "Reject successful";
    }
}