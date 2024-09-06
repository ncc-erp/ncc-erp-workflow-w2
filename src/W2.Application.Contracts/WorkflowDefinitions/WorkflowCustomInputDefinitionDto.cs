using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;
using W2.WorkflowInstances;

namespace W2.WorkflowDefinitions
{
    public class WorkflowCustomInputDefinitionDto : EntityDto<Guid>
    {
        public string WorkflowDefinitionId { get; set; }
        public SettingsEntity Settings { get; set; } = new SettingsEntity();
        public List<WorkflowCustomInputPropertyDefinitionDto> PropertyDefinitions { get; set; }
    }
  
    public class WorkflowCustomInputPropertyDefinitionDto
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsRequired { get; set; }
        public bool IsTitle { get; set; }
        public string TitleTemplate { get; set; }
    }
}
