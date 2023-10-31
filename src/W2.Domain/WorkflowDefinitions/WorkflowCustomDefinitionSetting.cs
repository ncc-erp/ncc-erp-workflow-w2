using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace W2.WorkflowDefinitions
{
    public class WorkflowCustomDefinitionSetting : CreationAuditedEntity<Guid>, IMultiTenant
    {
        public string WorkflowDefinitionId { get; set; }
        public ICollection<WorkflowCustomDefinitionPropertySetting> PropertyDefinitions { get; set; } = new List<WorkflowCustomDefinitionPropertySetting>();
        public Guid? TenantId { get; set; }
    }
}
