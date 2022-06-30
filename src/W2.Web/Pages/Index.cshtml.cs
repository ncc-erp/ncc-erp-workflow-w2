namespace W2.Web.Pages;

public class IndexModel : W2PageModel
{
    public async void OnGet()
    {
        var test = await SettingProvider.GetOrNullAsync(Volo.Abp.Account.Settings.AccountSettingNames.IsSelfRegistrationEnabled);
    }
}
