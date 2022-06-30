using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Expressions;
using Elsa.Services;
using Elsa.Services.Models;
using System.Threading.Tasks;

namespace W2.Web.Activities
{
    [Activity(Category = "Custom Activities", Description = "Get Assets Request")]
    public class GetAssetsRequest : Activity
    {
        [ActivityInput(
            Label = "Request",
            Hint = "Request Info",
            SupportedSyntaxes = new[] { SyntaxNames.Liquid, SyntaxNames.JavaScript })]
        public object Request { get; set; } = default!;

        protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            return Done(Request);
        }
    }
}
