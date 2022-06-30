using System;
using System.Collections.Generic;
using System.Text;
using W2.Localization;
using Volo.Abp.Application.Services;

namespace W2;

/* Inherit your application services from this class.
 */
public abstract class W2AppService : ApplicationService
{
    protected string CurrentTenantStrId => CurrentTenant?.Id?.ToString();

    protected W2AppService()
    {
        LocalizationResource = typeof(W2Resource);
    }
}
