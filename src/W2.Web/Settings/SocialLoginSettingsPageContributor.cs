using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System.Threading.Tasks;
using Volo.Abp.SettingManagement.Web.Pages.SettingManagement;
using W2.Localization;
using W2.Permissions;
using W2.Web.Pages.SettingManagement.Components.SocialLoginSettingGroup;

namespace W2.Web.Settings
{
    public class SocialLoginSettingsPageContributor : ISettingPageContributor
    {
        public async Task<bool> CheckPermissionsAsync(SettingPageCreationContext context)
        {
            var authorizationService = context.ServiceProvider.GetRequiredService<IAuthorizationService>();

            return await authorizationService.IsGrantedAsync(W2Permissions.WorkflowManagementSettingsSocialLoginSettings);
        }

        public async Task ConfigureAsync(SettingPageCreationContext context)
        {
            if (!await CheckPermissionsAsync(context))
            {
                return;
            }

            var l = context.ServiceProvider.GetRequiredService<IStringLocalizer<W2Resource>>();
            context.Groups.Add(
                new SettingPageGroup(
                    "W2.SocialLoginSettings",
                    l["Settings:SocialLoginSettings"],
                    typeof(SocialLoginSettingGroupViewComponent)
                )
            );
        }
    }
}
