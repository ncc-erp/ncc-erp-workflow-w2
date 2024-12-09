namespace W2.Web.ExceptionSubscriber;

using Volo.Abp.ExceptionHandling;
using Sentry;
using System.Threading.Tasks;

public class SentryExceptionSubscriber : ExceptionSubscriber
{
    public override Task HandleAsync(ExceptionNotificationContext context)
    {
        SentrySdk.CaptureException(context.Exception);
        return Task.CompletedTask;
    }
}