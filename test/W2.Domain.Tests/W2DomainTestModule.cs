using W2.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace W2;

[DependsOn(
    typeof(W2EntityFrameworkCoreTestModule)
    )]
public class W2DomainTestModule : AbpModule
{

}
