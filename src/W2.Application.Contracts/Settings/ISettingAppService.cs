using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using W2.WorkflowInstances;

namespace W2.Settings
{
    public interface ISettingAppService : IApplicationService
    {
        Task<SocialLoginSettingsDto> GetSocialLoginSettingsAsync();
        Task UpdateSocialLoginSettingsAsync(SocialLoginSettingsDto input);
        Task<W2SettingDto> GetW2SettingListAsync(string settingCode);
        Task<W2SettingDto> CreateNewW2SettingValueAsync(CreateNewW2SettingValueDto input);
        Task<W2SettingDto> UpdateW2SettingValueAsync(CreateNewW2SettingValueDto input);
        Task<bool> DeleteW2SettingValueAsync(CreateNewW2SettingValueDto input);
    }
}
