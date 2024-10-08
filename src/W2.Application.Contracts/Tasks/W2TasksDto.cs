﻿using System;
using System.Collections.Generic;
using Volo.Abp.Auditing;
using W2.TaskActions;
using W2.Tasks;
using W2.WorkflowDefinitions;

namespace W2.Tasks
{
    public class W2TasksDto
    {
        public Guid Id { get; set; }
        public string WorkflowInstanceId { get; set; }
        public string WorkflowDefinitionId { get; set; }
        public string Email { get; set; }
        public W2TaskStatus Status { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DynamicActionData { get; set; }
        public string Reason { get; set; }
        public DateTime CreationTime { get; set; }
        public string Title { get; set; }
        public Guid? RequestId { get; set; }
        public List<TaskActionsDto>? OtherActionSignals { get; set; }
        public List<string> EmailTo { get; set; }
        public Guid Author { get; set; }
        public string AuthorName { get; set; }
        public string UpdatedBy { get; set; }
        public SettingsDto Settings { get; set; }

    }
}
