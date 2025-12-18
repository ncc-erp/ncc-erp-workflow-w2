using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using W2.Activities;

namespace W2.WorkflowInstances
{
    public class RequestHistoryEventHandler(
        RequestHistoryManager requestHistoryManager) :
        ILocalEventHandler<RequestHistoryCreatedEvent>,
        ILocalEventHandler<RequestHistoryStatusChangedEvent>,
        ITransientDependency
    {
        private readonly RequestHistoryManager _requestHistoryManager = requestHistoryManager;

        public async Task HandleEventAsync(RequestHistoryCreatedEvent eventData)
        {
            await _requestHistoryManager.CreateHistoryRecordsAsync(eventData.Starter, eventData.Email);
        }

        public async Task HandleEventAsync(RequestHistoryStatusChangedEvent eventData)
        {
            await _requestHistoryManager.UpdateHistoryStatusAsync(eventData.WorkflowInstanceStarterId, eventData.NewStatus);
        }
    }
}
