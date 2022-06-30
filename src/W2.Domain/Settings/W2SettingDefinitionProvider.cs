using Volo.Abp.Settings;

namespace W2.Settings;

public class W2SettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(new SettingDefinition(W2Settings.SocialLoginSettingsEnableSocialLogin, "true"));
    }
}
