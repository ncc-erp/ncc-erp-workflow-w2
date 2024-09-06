﻿using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace W2.WorkflowDefinitions
{
    public class WorkflowCustomInputDefinition : CreationAuditedEntity<Guid>, IMultiTenant
    {
        public string WorkflowDefinitionId { get; set; }
        public SettingsDto Settings { get; set; } = new SettingsDto();
        public ICollection<WorkflowCustomInputPropertyDefinition> PropertyDefinitions { get; set; } = new List<WorkflowCustomInputPropertyDefinition>();
        public Guid? TenantId { get; set; }
    }

    public class SettingsDto
    {
        public string Color { get; set; }
    }
}
