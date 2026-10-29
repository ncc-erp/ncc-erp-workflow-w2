using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MimeKit;
using NSubstitute;
using Shouldly;
using W2.Configurations;
using W2.Email;
using Xunit;

namespace W2.Email;

public class AwsSesSmtpServiceTests
{
    [Fact]
    public async Task Should_Send_Raw_Mime_Message_Through_Aws_Ses()
    {
        SendEmailRequest capturedRequest = null;

        var sesClient = Substitute.For<IAmazonSimpleEmailServiceV2>();
        sesClient
            .SendEmailAsync(
                Arg.Do<SendEmailRequest>(request => capturedRequest = request),
                Arg.Any<CancellationToken>())
            .Returns(new SendEmailResponse
            {
                MessageId = "ses-message-id",
                HttpStatusCode = HttpStatusCode.OK
            });

        var configuration = Options.Create(new EmailConfiguration
        {
            Provider = EmailProviders.AwsSes,
            AwsSes = new AwsSesConfiguration
            {
                Region = "ap-southeast-1",
                FromAddress = "no-reply@example.com",
                FromDisplayName = "W2"
            }
        });

        var service = new AwsSesSmtpService(
            sesClient,
            configuration,
            NullLogger<AwsSesSmtpService>.Instance);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("W2", "no-reply@example.com"));
        message.To.Add(new MailboxAddress("User", "user@example.com"));
        message.Cc.Add(new MailboxAddress("Cc", "cc@example.com"));
        message.Subject = "Hello from W2";
        message.Body = new TextPart("html") { Text = "<b>Hello</b>" };

        await service.SendAsync(null, message, CancellationToken.None);

        capturedRequest.ShouldNotBeNull();
        capturedRequest.FromEmailAddress.ShouldContain("no-reply@example.com");
        capturedRequest.Destination.ToAddresses.ShouldContain("user@example.com");
        capturedRequest.Destination.CcAddresses.ShouldContain("cc@example.com");
        capturedRequest.Content.Raw.Data.ShouldNotBeNull();
    }
}
