using Microsoft.Extensions.Configuration;
using System;
using Volo.Abp.Emailing;
using Volo.Abp.Settings;
using W2.Configurations;

namespace W2.Settings;

public class W2SettingDefinitionProvider : SettingDefinitionProvider
{
    private readonly IConfiguration _configuration;

    public W2SettingDefinitionProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(new SettingDefinition(W2Settings.SocialLoginSettingsEnableSocialLogin, "true"));
        ConfigureDefaultSmtpSettings(context);
    }

    private void ConfigureDefaultSmtpSettings(ISettingDefinitionContext context)
    {
        var elsaConfiguration = _configuration.GetSection(nameof(ElsaConfiguration)).Get<ElsaConfiguration>();
        
        var smtpHost = context.GetOrNull(EmailSettingNames.Smtp.Host);
        if (smtpHost != null)
        {
            smtpHost.DefaultValue = elsaConfiguration.Smtp.Host;
        }
        var smtpPort = context.GetOrNull(EmailSettingNames.Smtp.Port);
        if (smtpPort != null)
        {
            smtpPort.DefaultValue = elsaConfiguration.Smtp.Port;
        }
        var smtpUserName = context.GetOrNull(EmailSettingNames.Smtp.UserName);
        if (smtpUserName != null)
        {
            smtpUserName.DefaultValue = elsaConfiguration.Smtp.UserName;
        }
        var smtpPassword = context.GetOrNull(EmailSettingNames.Smtp.Password);
        if (smtpPassword != null)
        {
            smtpPassword.DefaultValue = elsaConfiguration.Smtp.Password;
        }
        var smtpDefaulFromAddress = context.GetOrNull(EmailSettingNames.DefaultFromAddress);
        if (smtpDefaulFromAddress != null)
        {
            smtpDefaulFromAddress.DefaultValue = elsaConfiguration.Smtp.DefaultSender;
        }
        var smtpUseDefaultCredentials = context.GetOrNull(EmailSettingNames.Smtp.UseDefaultCredentials);
        if (smtpUseDefaultCredentials != null)
        {
            smtpUseDefaultCredentials.DefaultValue = (!elsaConfiguration.Smtp.RequireCredentials).ToString();
        }
    }
}
