using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using W2.Activities;

namespace W2.WorkflowInstances
{
    public class RequestEventHandler :
        ILocalEventHandler<RequestCreatedEvent>,
        ILocalEventHandler<RequestStatusChangedEvent>,
        ITransientDependency
    {
        private readonly RequestHistoryManager _requestHistoryManager;

        public RequestEventHandler(RequestHistoryManager requestHistoryManager)
        {
            _requestHistoryManager = requestHistoryManager;
        }

        public async Task HandleEventAsync(RequestCreatedEvent eventData)
        {
            await _requestHistoryManager.CreateHistoryRecordsAsync(eventData.Starter, eventData.Email);
        }

        public async Task HandleEventAsync(RequestStatusChangedEvent eventData)
        {
            await _requestHistoryManager.UpdateHistoryStatusAsync(eventData.WorkflowInstanceStarterId, eventData.NewStatus);
        }
    }
}
