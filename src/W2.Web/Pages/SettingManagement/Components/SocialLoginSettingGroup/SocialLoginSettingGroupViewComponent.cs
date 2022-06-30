using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;
using W2.Settings;

namespace W2.Web.Pages.SettingManagement.Components.SocialLoginSettingGroup
{
    public class SocialLoginSettingGroupViewComponent : AbpViewComponent
    {
        private readonly ISettingAppService _settingAppService;

        public SocialLoginSettingGroupViewComponent(ISettingAppService settingAppService)
        {
            _settingAppService = settingAppService;
        }

        public virtual async Task<IViewComponentResult> InvokeAsync()
        {
            var model = ObjectMapper.Map<SocialLoginSettingsDto, UpdateSocialLoginSettingsViewModel>(
                await _settingAppService.GetSocialLoginSettingsAsync()
            );
            return View("~/Pages/SettingManagement/Components/SocialLoginSettingGroup/Default.cshtml", model);
        }

        public class UpdateSocialLoginSettingsViewModel
        {
            [Display(Name = "Settings:SocialLoginSettings:EnableSocialLogin")]
            public bool EnableSocialLogin { get; set; }
        }
    }
}
