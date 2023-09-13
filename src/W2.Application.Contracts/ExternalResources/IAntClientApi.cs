using Refit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace W2.ExternalResources
{
    public interface IAntClientApi : IApplicationService
    {
        [Get("/wp-json/wp/v2/users")]
        Task<List<UserInfoBySlug>> GetUsersBySlugAsync([AliasAs("slug")] string slug);

        [Get("/wp-json/wp/v2/posts")]
        Task<List<PostItem>> GetPostsByUserIdAsync([AliasAs("author")] int author);
    }

}
