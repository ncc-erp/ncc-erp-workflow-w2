using AutoMapper;
using Elsa.Models;
using System;
using System.Linq;
using W2.WorkflowDefinitions;
using W2.WorkflowInstances;

namespace W2;

public class W2ApplicationAutoMapperProfile : Profile
{
    public W2ApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */
        CreateMap<WorkflowDefinition, WorkflowDefinitionSummaryDto>();
        CreateMap<WorkflowCustomInputPropertyDefinition, WorkflowCustomInputPropertyDefinitionDto>();
        CreateMap<WorkflowCustomInputDefinition, WorkflowCustomInputDefinitionDto>()
            .ForMember(d => d.PropertyDefinitions, options => 
                options.MapFrom(s => s.PropertyDefinitions.Select(i => new WorkflowCustomInputPropertyDefinitionDto
                {
                    Name = i.Name,
                    Type = i.Type
                })
                .ToList()));
        CreateMap<WorkflowCustomInputDefinitionDto, WorkflowCustomInputDefinition>()
            .ForMember(d => d.PropertyDefinitions, options =>
                options.MapFrom(s => s.PropertyDefinitions.Select(i => new WorkflowCustomInputPropertyDefinition
                {
                    Name = i.Name,
                    Type = i.Type
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
    }
}
