using Elsa.Scripting.JavaScript.Services;
using System;
using System.Collections.Generic;
using Volo.Abp.Users;
using W2.ExternalResources;

namespace W2.Scripting
{
    public class CustomTypeDefinitionProvider : TypeDefinitionProvider
    {
        public override IEnumerable<Type> CollectTypes(TypeDefinitionContext context)
        {
            return new[] 
            { 
                typeof(WorkflowSignals),
                typeof(ICurrentUser),
                typeof(OfficeInfo),
                typeof(SignalInputTypes),
                typeof(RequestUser),
                typeof(WorkflowInstanceVariableNames),
                typeof(TimesheetProjectItem),
                typeof(ProjectManager)
            };
        }
    }

    public class WorkflowSignals
    {
        public string PMApproved { get; set; } = nameof(PMApproved);
        public string PMRejected { get; set; } = nameof(PMRejected);
        public string HPMApproved { get; set; } = nameof(HPMApproved);
        public string HPMRejected { get; set; } = nameof(HPMRejected);
        public string HoOApproved { get; set; } = nameof(HoOApproved);
        public string HoORejected { get; set; } = nameof(HoORejected);
        public string Approved { get; set; } = nameof(Approved);
        public string Rejected { get; set; } = nameof(Rejected);
        public string CustomerApproved { get; set; } = nameof(CustomerApproved);
        public string CustomerRejected { get; set; } = nameof(CustomerRejected);
        public string ProceedInternal { get; set; } = nameof(ProceedInternal);
        public string SentToCustomer { get; set; } = nameof(SentToCustomer);
        public string CEOApproved { get; set; } = nameof(CEOApproved);
        public string CEORejected { get; set; } = nameof(CEORejected);
    }

    public class SignalInputTypes
    {
        public string Reason { get; } = SignalInputType.Reason;
        public string TriggeredBy { get; } = SignalInputType.TriggeredBy;
    }

    public static class SignalInputType
    {
        public const string Reason = nameof(Reason);
        public const string TriggeredBy = nameof(TriggeredBy);
    }

    public class WorkflowInstanceVariableNames
    {
        public string Request { get; set; } = nameof(Request);
        public string RequestUser { get; set; } = nameof(RequestUser);
    }
}
