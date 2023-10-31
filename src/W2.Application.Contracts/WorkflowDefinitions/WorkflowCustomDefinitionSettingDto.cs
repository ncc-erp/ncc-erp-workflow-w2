using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace W2.WorkflowDefinitions
{
    public class WorkflowCustomDefinitionSettingDto : EntityDto<Guid>
    {
        public string WorkflowDefinitionId { get; set; }
        public List<WorkflowCustomDefinitionPropertySettingDto> PropertyDefinitions { get; set; }
    }

    public class WorkflowCustomDefinitionPropertySettingDto
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
