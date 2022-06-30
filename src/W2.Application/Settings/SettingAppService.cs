using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using Volo.Abp.SettingManagement;
using Volo.Abp.Settings;
using W2.Permissions;

namespace W2.Settings
{
    [Authorize]
    public class SettingAppService : W2AppService, ISettingAppService
    {
        private readonly ISettingManager _settingManager;

        public SettingAppService(ISettingManager settingManager)
        {
            _settingManager = settingManager;
        }

        public async Task<SocialLoginSettingsDto> GetSocialLoginSettingsAsync()
        {
            var socialLoginSettingsDto = new SocialLoginSettingsDto();

            if (CurrentTenant.IsAvailable)
            {
                socialLoginSettingsDto.EnableSocialLogin = Convert.ToBoolean(await _settingManager.GetOrNullForCurrentTenantAsync(W2Settings.SocialLoginSettingsEnableSocialLogin));
            }
            else
            {
                socialLoginSettingsDto.EnableSocialLogin = Convert.ToBoolean(await _settingManager.GetOrNullGlobalAsync(W2Settings.SocialLoginSettingsEnableSocialLogin));
            }

            return socialLoginSettingsDto;
        }

        [Authorize(W2Permissions.WorkflowManagementSettingsSocialLoginSettings)]
        public async Task UpdateSocialLoginSettingsAsync(SocialLoginSettingsDto input)
        {
            await _settingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, W2Settings.SocialLoginSettingsEnableSocialLogin, input.EnableSocialLogin.ToString());
        }
    }
}
