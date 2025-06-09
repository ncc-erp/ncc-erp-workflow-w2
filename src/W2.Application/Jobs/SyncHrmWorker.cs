

using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Threading;
using W2.Users;

namespace W2.Jobs
{
    public class SyncHrmWorker : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        public SyncHrmWorker(
            AbpTimer timer,
            IServiceScopeFactory serviceScopeFactory
        ) : base(timer, serviceScopeFactory)
        {
            Timer.Period = 60 * 60 * 1000; // 1 hour in milliseconds
        }

        protected override void DoWork(PeriodicBackgroundWorkerContext workerContext)
        {
            var _userAppService = LazyServiceProvider.LazyGetService<IUserAppService>();
            _userAppService.InternalSyncHrmUsers();
        }
    }
}
