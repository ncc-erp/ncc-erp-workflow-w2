using Elsa.Services;
using Elsa.Services.Models;
using System.Linq;
using W2.Scripting;

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

        public static RequestUser GetRequestUserVariable(this ActivityExecutionContext context)
        {
            return context.GetVariable<RequestUser>(nameof(RequestUser));
        }
    }
}
