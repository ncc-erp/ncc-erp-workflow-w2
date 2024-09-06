using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using W2.WorkflowInstances;

namespace W2.WorkflowDefinitions
{
    public class WorkflowCustomInputDefinition : CreationAuditedEntity<Guid>, IMultiTenant
    {
        public string WorkflowDefinitionId { get; set; }
        public SettingsEntity Settings { get; set; } = new SettingsEntity();
        public ICollection<WorkflowCustomInputPropertyDefinition> PropertyDefinitions { get; set; } = new List<WorkflowCustomInputPropertyDefinition>();
        public Guid? TenantId { get; set; }
    }
}
