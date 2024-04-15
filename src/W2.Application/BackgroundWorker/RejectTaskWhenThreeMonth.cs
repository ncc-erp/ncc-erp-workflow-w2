using System.Threading.Tasks;
using Quartz;
using Volo.Abp.BackgroundWorkers.Quartz;
using W2.Tasks;

namespace W2.BackgroundWorker
{
    public class InterestRecordsJob : QuartzBackgroundWorkerBase
    {
        private TaskAppService _taskAppService;
        public InterestRecordsJob(TaskAppService taskAppService)
        {
            _taskAppService = taskAppService;
            JobDetail = JobBuilder.Create<InterestRecordsJob>().WithIdentity(nameof(InterestRecordsJob)).Build();


            Trigger = TriggerBuilder.Create().WithIdentity(nameof(InterestRecordsJob)).StartNow().WithCronSchedule("0 0 1 * * ?").Build();


        }
        public override async Task Execute(IJobExecutionContext context)
        {
            await _taskAppService.UpdateTasksAtMidnightAsync();
        }
    }
}
