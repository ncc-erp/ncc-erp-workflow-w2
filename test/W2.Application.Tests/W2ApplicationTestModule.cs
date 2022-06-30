using Volo.Abp.Modularity;

namespace W2;

[DependsOn(
    typeof(W2ApplicationModule),
    typeof(W2DomainTestModule)
    )]
public class W2ApplicationTestModule : AbpModule
{

}
