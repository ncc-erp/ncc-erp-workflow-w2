﻿using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using W2.WorkflowInstances;

namespace W2.WorkflowDefinitions
{
    public class WorkflowCustomInputDefinition : CreationAuditedEntity<Guid>, IMultiTenant
    {
        public string WorkflowDefinitionId { get; set; }
        public Settings Settings { get; set; } = new Settings();
        public ICollection<WorkflowCustomInputPropertyDefinition> PropertyDefinitions { get; set; } = new List<WorkflowCustomInputPropertyDefinition>();
        public Guid? TenantId { get; set; }
    }

    public class Settings
    {
        public string Color { get; set; }
        public string TitleTemplate { get; set; }
        public bool IsSendKomuMessage { get; set; }
    }
}
