using Elsa.Services;
using Elsa.Services.Models;
using System.Collections.Generic;
using System.Linq;

namespace W2.Signals
{
    public static class CustomSignalExtensions
    {
        public static string GenerateSignalTokenWithForm(this ActivityExecutionContext context, string signal, string[] requiredInputs)
        {
            string id = context.WorkflowExecutionContext.WorkflowInstance.Id;
            var payload = new SignalModelDto
            {
                Name = signal,
                WorkflowInstanceId = id,
                RequiredInputs = requiredInputs.ToList()
            };
            ITokenService service = context.GetService<ITokenService>();
            return service.CreateToken(payload);
        }
    }
}
