using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace W2.Pages;

public class Index_Tests : W2WebTestBase
{
    [Fact]
    public async Task Welcome_Page()
    {
        var response = await GetResponseAsStringAsync("/");
        response.ShouldNotBeNull();
    }
}
