using AutoMapper;
using W2.Settings;
using W2.Web.Pages.WorkflowDefinitions.Models;
using W2.WorkflowDefinitions;

namespace W2.Web;

public class W2WebAutoMapperProfile : Profile
{
    public W2WebAutoMapperProfile()
    {
        //Define your AutoMapper configuration here for the Web project.
        CreateMap<WorkflowCustomInputPropertyDefinitionViewModel, WorkflowCustomInputPropertyDefinitionDto>();
        CreateMap<DefineWorkflowInputViewModel, WorkflowCustomInputDefinitionDto>()
            .ForMember(d => d.PropertyDefinitions, options => options.MapFrom(s => s.PropertyDefinitionViewModels));
        CreateMap<SocialLoginSettingsDto, Pages.SettingManagement.Components.SocialLoginSettingGroup.SocialLoginSettingGroupViewComponent.UpdateSocialLoginSettingsViewModel>();
    }
}
