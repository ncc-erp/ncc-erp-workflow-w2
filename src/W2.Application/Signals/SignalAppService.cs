using Elsa.Activities.Http.Events;
using Elsa.Activities.Signaling.Models;
using Elsa.Activities.Signaling.Services;
using Elsa.Services;
using MediatR;
using Open.Linq.AsyncExtensions;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;

namespace W2.Signals
{
    public class SignalAppService : W2AppService, ISignalAppService
    {
        private readonly ISignaler _signaler;
        private readonly ITokenService _tokenService;
        private readonly IMediator _mediator;

        public SignalAppService(ISignaler signaler, 
            ITokenService tokenService, 
            IMediator mediator)
        {
            _signaler = signaler;
            _tokenService = tokenService;
            _mediator = mediator;
        }

        public async Task TriggerAsync(string token)
        {
            if (!_tokenService.TryDecryptToken(token, out SignalModel signal))
            {
                throw new UserFriendlyException(L["Exception:InvalidSignalToken"]);
            }

            var affectedWorkflows = await _signaler.TriggerSignalAsync(signal.Name, null, signal.WorkflowInstanceId).ToList();

            await _mediator.Publish(new HttpTriggeredSignal(signal, affectedWorkflows));
        }
    }
}
