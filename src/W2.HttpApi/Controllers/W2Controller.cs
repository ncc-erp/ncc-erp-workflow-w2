using W2.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace W2.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class W2Controller : AbpControllerBase
{
    protected W2Controller()
    {
        LocalizationResource = typeof(W2Resource);
    }
}
