using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.SettingManagement;
using W2.Authorization.Attributes;
using W2.Constants;
using W2.Permissions;

namespace W2.Settings
{
    //[Authorize]
    [RequirePermission(W2ApiPermissions.SettingsManagement)]
    public class SettingAppService : W2AppService, ISettingAppService
    {
        private readonly ISettingManager _settingManager;
        private readonly IRepository<W2Setting, Guid> _settingRepository;

        public SettingAppService(ISettingManager settingManager, IRepository<W2Setting, Guid> settingRepository)
        {
            _settingManager = settingManager;
            _settingRepository = settingRepository;
        }

        [Authorize(W2Permissions.WorkflowManagementSettingsSocialLoginSettings)]
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

        [RequirePermission(W2ApiPermissions.ViewListSettings)]
        public async Task<W2SettingDto> GetW2SettingListAsync(string settingCode)
        {
            var w2Setting = await _settingRepository.FirstOrDefaultAsync(setting => setting.Code == settingCode);
            var config = new MapperConfiguration(cfg => cfg.CreateMap<W2Setting, W2SettingDto>());
            var mapper = config.CreateMapper();
            var w2settingDto = mapper.Map<W2SettingDto>(w2Setting);
            return w2settingDto;
        }

        [RequirePermission(W2ApiPermissions.CreateSetting)]
        public async Task<W2SettingDto> CreateNewW2SettingValueAsync(CreateNewW2SettingValueDto input)
        {
            var setting = await _settingRepository.FirstOrDefaultAsync(u => u.Code == input.SettingCode);
            var config = new MapperConfiguration(cfg => cfg.CreateMap<W2Setting, W2SettingDto>());
            var mapper = config.CreateMapper();
            if (setting == null)
            {
                var newSettingValue = new W2SettingValue
                {
                    items = new List<W2SettingValueItem>
                    {
                        new W2SettingValueItem { code = input.Code, email = input.Email, name = input.Name }
                    }
                };

                var newW2Setting = new W2Setting
                {
                    Code = input.SettingCode,
                    Name = input.SettingCode + "W2Settings",
                    ValueObject = newSettingValue
                };

                setting = await _settingRepository.InsertAsync(newW2Setting);
                var w2settingDto = mapper.Map<W2SettingDto>(setting);
                return w2settingDto;
            } else
            {
                var settingValue = setting.ValueObject;

                if (input.SettingCode == SettingCodeEnum.DIRECTOR)
                {
                    var duplicateCodeItem = settingValue.items.FirstOrDefault(item => item.code == input.Code);
                    if (duplicateCodeItem != null)
                    {
                        throw new UserFriendlyException("Exception:W2SettingCodeDuplicate", "409");
                    }
                }
                else
                {
                    var duplicateSettingItem = settingValue.items.FirstOrDefault(item => item.email == input.Email);
                    if (duplicateSettingItem != null)
                    {
                        throw new UserFriendlyException("Exception:W2SettingMailDuplicate", "409");
                    }
                }

                settingValue.items.Add(new W2SettingValueItem { code = input.Code, email = input.Email, name = input.Name});
                setting.ValueObject = settingValue;
                var newSetting = await _settingRepository.UpdateAsync(setting);
                var w2settingDto = mapper.Map<W2SettingDto>(newSetting);
                return w2settingDto;
            }
        }

        [RequirePermission(W2ApiPermissions.UpdateSetting)]
        public async Task<W2SettingDto> UpdateW2SettingValueAsync(CreateNewW2SettingValueDto input)
        {
            var setting = await _settingRepository.FirstOrDefaultAsync(u => u.Code == input.SettingCode);
            if (setting == null)
            {
                throw new UserFriendlyException(L["Exception:W2SettingNotFound"]);
            }
            
            var settingValue = setting.ValueObject;
            var updateSettingValue = new W2SettingValueItem(); 

            if (input.SettingCode == SettingCodeEnum.DIRECTOR) updateSettingValue = settingValue.items.FirstOrDefault(u => u.code == input.Code);
            else updateSettingValue = settingValue.items.FirstOrDefault(u => u.email == input.Email);

            if (updateSettingValue == null)
            {
                throw new UserFriendlyException(L["Exception:W2SettingValueItemNotFound"]);
            }
            updateSettingValue.name = input.Name;
            updateSettingValue.code = input.Code;
            updateSettingValue.email = input.Email;
            setting.ValueObject = settingValue;
            var newSetting = await _settingRepository.UpdateAsync(setting);
            var config = new MapperConfiguration(cfg => cfg.CreateMap<W2Setting, W2SettingDto>());
            var mapper = config.CreateMapper();
            var w2settingDto = mapper.Map<W2SettingDto>(newSetting);
            return w2settingDto;
            
        }

        [RequirePermission(W2ApiPermissions.DeleteSetting)]
        public async Task<bool> DeleteW2SettingValueAsync(CreateNewW2SettingValueDto input)
        {
            var setting = await _settingRepository.FirstOrDefaultAsync(u => u.Code == input.SettingCode);
            if (setting == null)
            {
                throw new UserFriendlyException(L["Exception:W2SettingNotFound"]);
            }
            var settingValue = setting.ValueObject;
            var deleteSettingValue = new W2SettingValueItem(); 

            if (input.SettingCode == SettingCodeEnum.DIRECTOR) deleteSettingValue = settingValue.items.FirstOrDefault(u => u.code == input.Code);
            else deleteSettingValue = settingValue.items.FirstOrDefault(u => u.email == input.Email);

            if (deleteSettingValue == null)
            {
                throw new UserFriendlyException(L["Exception:W2SettingValueItemNotFound"]);
            }
            settingValue.items.Remove(deleteSettingValue);
            setting.ValueObject = settingValue;
            await _settingRepository.UpdateAsync(setting);
            return true;
        }
    }
}
