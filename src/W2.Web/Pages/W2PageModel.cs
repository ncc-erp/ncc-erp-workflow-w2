using W2.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace W2.Web.Pages;

/* Inherit your PageModel classes from this class.
 */
public abstract class W2PageModel : AbpPageModel
{
    protected W2PageModel()
    {
        LocalizationResourceType = typeof(W2Resource);
    }
}
