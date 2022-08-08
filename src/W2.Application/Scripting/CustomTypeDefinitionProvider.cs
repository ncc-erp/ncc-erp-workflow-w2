using Elsa.Scripting.JavaScript.Services;
using System;
using System.Collections.Generic;

namespace W2.Scripting
{
    public class CustomTypeDefinitionProvider : TypeDefinitionProvider
    {
        public override IEnumerable<Type> CollectTypes(TypeDefinitionContext context)
        {
            return new[] { typeof(WorkflowSignals) };
        }
    }

    public class WorkflowSignals
    {
        public string PMApproved { get; set; } = nameof(PMApproved);
        public string PMRejected { get; set; } = nameof(PMRejected);
        public string HoOApproved { get; set; } = nameof(HoOApproved);
        public string HoORejected { get; set; } = nameof(HoORejected);
        public string Approved { get; set; } = nameof(Approved);
        public string Rejected { get; set; } = nameof(Rejected);
        public string CustomerApproved { get; set; } = nameof(CustomerApproved);
        public string CustomerRejected { get; set; } = nameof(CustomerRejected);
        public string ProceedInternal { get; set; } = nameof(ProceedInternal);
        public string SentToCustomer { get; set; } = nameof(SentToCustomer);

    }
}
