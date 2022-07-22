using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;

namespace W2.Web;

[Dependency(ReplaceServices = true)]
public class W2BrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "NCC Workflow";
    public override string LogoUrl => "/logo.png";
}
