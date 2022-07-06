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
    }
}
