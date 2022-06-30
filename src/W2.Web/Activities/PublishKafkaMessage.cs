using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Expressions;
using Elsa.Services;
using Elsa.Services.Models;
using System.Threading.Tasks;

namespace W2.Web.Activities
{
    [Activity(Category = "Custom Activities", Description = "Publish message to kafka")]
    public class PublishKafkaMessage : Activity
    {
        public PublishKafkaMessage()
        {
        }

        [ActivityInput(
            Label = "Message Object",
            Hint = "Object to attach to message",
            SupportedSyntaxes = new[] { SyntaxNames.Literal, SyntaxNames.JavaScript, SyntaxNames.Liquid, SyntaxNames.Variable }
        )]
        public object MessageObject { get; set; }

        [ActivityInput(
            Label = "Message Type",
            Hint = "Message Type",
            SupportedSyntaxes = new[] { SyntaxNames.Literal }
        )]
        public string MessageType { get; set; } = string.Empty;

        protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            return Done();
        }
    }
}
