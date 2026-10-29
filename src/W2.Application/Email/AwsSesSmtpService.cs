using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Elsa.Activities.Email.Services;
using Elsa.Services.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Volo.Abp;
using W2.Configurations;

namespace W2.Email;

/// <summary>
/// Sends Elsa e-mail activities through the AWS SES v2 API instead of SMTP.
/// The MIME message built by Elsa is sent as a raw SES e-mail, so all existing
/// features (HTML body, CC/BCC, attachments) keep working.
/// </summary>
public class AwsSesSmtpService : ISmtpService
{
    private readonly IAmazonSimpleEmailServiceV2 _sesClient;
    private readonly AwsSesConfiguration _configuration;
    private readonly ILogger<AwsSesSmtpService> _logger;

    public AwsSesSmtpService(
        IAmazonSimpleEmailServiceV2 sesClient,
        IOptions<EmailConfiguration> emailConfiguration,
        ILogger<AwsSesSmtpService> logger)
    {
        _sesClient = sesClient;
        _configuration = emailConfiguration.Value.AwsSes ?? new AwsSesConfiguration();
        _logger = logger;

        _logger.LogInformation(
            "AWS SES e-mail sender initialized. Region: {Region}, ConfigurationSet: {ConfigurationSet}",
            _configuration.Region,
            _configuration.ConfigurationSetName);
    }

    public async Task SendAsync(ActivityExecutionContext context, MimeMessage message, CancellationToken cancellationToken)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        var toAddresses = message.To.Mailboxes
            .Select(x => x.Address)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        var ccAddresses = message.Cc.Mailboxes
            .Select(x => x.Address)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        var bccAddresses = message.Bcc.Mailboxes
            .Select(x => x.Address)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        if (toAddresses.Count == 0 && ccAddresses.Count == 0 && bccAddresses.Count == 0)
        {
            throw new AbpException("Cannot send the e-mail: no recipient address was provided.");
        }

        var destination = new Destination();

        if (toAddresses.Count > 0)
        {
            destination.ToAddresses = toAddresses;
        }

        if (ccAddresses.Count > 0)
        {
            destination.CcAddresses = ccAddresses;
        }

        if (bccAddresses.Count > 0)
        {
            destination.BccAddresses = bccAddresses;
        }

        using var rawMessage = new MemoryStream();
        await message.WriteToAsync(rawMessage, cancellationToken);
        rawMessage.Position = 0;

        var request = new SendEmailRequest
        {
            FromEmailAddress = GetFromEmailAddress(message),
            Destination = destination,
            Content = new EmailContent
            {
                Raw = new RawMessage
                {
                    Data = rawMessage
                }
            },
            ConfigurationSetName = string.IsNullOrWhiteSpace(_configuration.ConfigurationSetName)
                ? null
                : _configuration.ConfigurationSetName
        };

        try
        {
            var response = await _sesClient.SendEmailAsync(request, cancellationToken);

            _logger.LogInformation(
                "E-mail sent via AWS SES. MessageId: {MessageId}, Recipients: {Recipients}",
                response.MessageId,
                string.Join(", ", toAddresses.Concat(ccAddresses).Concat(bccAddresses)));
        }
        catch (AmazonSimpleEmailServiceV2Exception ex)
        {
            _logger.LogError(
                ex,
                "AWS SES failed to send e-mail. ErrorCode: {ErrorCode}, Message: {ErrorMessage}",
                ex.ErrorCode,
                ex.Message);

            throw;
        }
    }

    private string GetFromEmailAddress(MimeMessage message)
    {
        MailboxAddress fromMailbox = null;

        if (!string.IsNullOrWhiteSpace(_configuration.FromAddress))
        {
            fromMailbox = MailboxAddress.Parse(_configuration.FromAddress.Trim());
        }

        fromMailbox ??= message.From.Mailboxes.FirstOrDefault();

        if (fromMailbox == null || string.IsNullOrWhiteSpace(fromMailbox.Address))
        {
            throw new AbpException(
                "Cannot send the e-mail: no sender address was provided. Configure " +
                "'Email:AwsSes:FromAddress' or 'ElsaConfiguration:Smtp:DefaultSender'.");
        }

        var displayName = string.IsNullOrWhiteSpace(_configuration.FromDisplayName)
            ? fromMailbox.Name
            : _configuration.FromDisplayName.Trim();

        return string.IsNullOrWhiteSpace(displayName)
            ? fromMailbox.Address
            : new MailboxAddress(displayName, fromMailbox.Address).ToString();
    }
}
