﻿using System;
using System.Collections.Generic;
using Volo.Abp.Auditing;
using W2.Tasks;

namespace W2.Tasks
{
    public class W2TasksDto 
    {
        public string Id { get; set; }
        public string WorkflowInstanceId { get; set; } = default!;
        public string Email { get; set; }
        public W2TaskStatus Status { get; set; }
        public string Name { get; set; } 
        public string Reason { get; set; } 
        public string ApproveSignal { get; set; }
        public string RejectSignal { get; set; } 
    }
}
