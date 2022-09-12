using Elsa.Activities.Http.Events;
using Elsa.Activities.Signaling.Models;
using Elsa.Activities.Signaling.Services;
using Elsa.Services;
using MediatR;
using Open.Linq.AsyncExtensions;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Users;
using W2.Scripting;

namespace W2.Signals
{
    public class SignalAppService : W2AppService, ISignalAppService
    {
        private readonly ISignaler _signaler;
        private readonly ITokenService _tokenService;
        private readonly IMediator _mediator;
        private readonly ICurrentUser _currentUser;

        public SignalAppService(ISignaler signaler,
            ITokenService tokenService,
            IMediator mediator,
            ICurrentUser currentUser)
        {
            _signaler = signaler;
            _tokenService = tokenService;
            _mediator = mediator;
            _currentUser = currentUser;
        }

        public Task<SignalModelDto> GetSignalModelFromTokenAsync(string token)
        {
            if (!_tokenService.TryDecryptToken(token, out SignalModelDto signalModel))
            {
                throw new UserFriendlyException(L["Exception:InvalidSignalToken"]);
            }

            return Task.FromResult(signalModel);
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

        public async Task TriggerAsync(TriggerSignalWithInputDto triggerSignalInput)
        {
            if (triggerSignalInput.Inputs.ContainsKey(SignalInputType.TriggeredBy))
            {
                triggerSignalInput.Inputs[SignalInputType.TriggeredBy] = $"{_currentUser.Name} ({_currentUser.Email})";
            }
            var affectedWorkflows = await _signaler.TriggerSignalAsync
                (
                    triggerSignalInput.Signal, 
                    triggerSignalInput.Inputs, 
                    triggerSignalInput.WorkflowInstanceId
                )
                .ToList();
            var signalModel = new SignalModel(triggerSignalInput.Signal, triggerSignalInput.WorkflowInstanceId);
            await _mediator.Publish(new HttpTriggeredSignal(signalModel, affectedWorkflows));
        }
    }
}
