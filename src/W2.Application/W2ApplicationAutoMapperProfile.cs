using AutoMapper;
using Elsa.Models;
using System;
using System.Linq;
using W2.Tasks;
using W2.WorkflowDefinitions;
using W2.WorkflowInstances;
using W2.Permissions;
using W2.Roles;
using Volo.Abp.Identity;
using W2.Identity;
using W2.Users;

namespace W2;

public class W2ApplicationAutoMapperProfile : Profile
{
    public W2ApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */
        CreateMap<W2Task, W2TasksDto>();
        CreateMap<WorkflowDefinition, WorkflowDefinitionSummaryDto>();
        CreateMap<WorkflowCustomInputPropertyDefinition, WorkflowCustomInputPropertyDefinitionDto>();
        CreateMap<WorkflowCustomInputDefinition, WorkflowCustomInputDefinitionDto>()
            .ForMember(d => d.PropertyDefinitions, options =>
                options.MapFrom(s => s.PropertyDefinitions.Select(i => new WorkflowCustomInputPropertyDefinitionDto
                {
                    Name = i.Name,
                    Type = i.Type,
                    IsRequired = i.IsRequired
                })
                .ToList()));
        CreateMap<WorkflowCustomInputDefinitionDto, WorkflowCustomInputDefinition>()
            .ForMember(d => d.PropertyDefinitions, options =>
                options.MapFrom(s => s.PropertyDefinitions.Select(i => new WorkflowCustomInputPropertyDefinition
                {
                    Name = i.Name,
                    Type = i.Type,
                    IsRequired = i.IsRequired
                })
                .ToList()));
        CreateMap<CreateWorkflowDefinitionDto, WorkflowDefinition>();
        CreateMap<WorkflowInstance, WorkflowInstanceDto>()
            .ForMember(d => d.WorkflowDefinitionId, options => options.MapFrom(s => s.DefinitionId))
            .ForMember(d => d.CreatedAt, options => options.MapFrom(s => s.CreatedAt.ToDateTimeUtc()))
            .ForMember(d => d.Status, options => options.MapFrom(s => s.WorkflowStatus == WorkflowStatus.Suspended ? "Pending" : s.WorkflowStatus.ToString()))
            .ForMember(d => d.LastExecutedAt, options => options.MapFrom((s, d) =>
            {
                return s.LastExecutedAt.HasValue ? s.LastExecutedAt.Value.ToDateTimeUtc() : (DateTime?)null;
            }));
        CreateMap<WorkflowInstance, WorkflowStatusDto>()
            // .ForMember(d => d.CreatedAt, options => options.MapFrom(s => s.CreatedAt.ToDateTimeUtc()))
            .ForMember(d => d.Status, options => options.MapFrom(s => GetMappedStatus(s.WorkflowStatus)));
        CreateMap<W2.WorkflowDefinitions.Settings, SettingsDto>();
        CreateMap<SettingsDto, W2.WorkflowDefinitions.Settings>();
        CreateMap<W2Permission, PermissionDto>();
        CreateMap<W2Permission, PermissionDetailDto>()
            .ForMember(dest => dest.Children, opt => opt.Ignore());
        CreateMap<IdentityRole, RoleDetailDto>()
            .ForMember(dest => dest.Permissions, opt => opt.Ignore());
        CreateMap<W2CustomIdentityUser, UserDto>()
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src =>
                src.UserRoles.Select(ur => ur.Role.Name).ToList()));
        CreateMap<W2CustomIdentityUser, UserDetailDto>()
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src =>
                src.UserRoles.Select(ur => ur.Role.Name).ToList()))
            .ForMember(dest => dest.CustomPermissions, opt => opt.MapFrom(src =>
                src.CustomPermissionDtos));
    }
    private int GetMappedStatus(WorkflowStatus workflowStatus)
    {
        if (workflowStatus == WorkflowStatus.Suspended)
        {
            return 0;
        }
        return workflowStatus.ToString().ToLower() switch
        {
            "finished" => 1,
            "rejected" => 2,
            _ => 0,
        };
    }
}
