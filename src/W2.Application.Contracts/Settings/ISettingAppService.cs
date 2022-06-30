using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace W2.Settings
{
    public interface ISettingAppService : IApplicationService
    {
        Task<SocialLoginSettingsDto> GetSocialLoginSettingsAsync();
        Task UpdateSocialLoginSettingsAsync(SocialLoginSettingsDto input);
    }
}
