using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace W2.ExternalResources
{
    public class DummyHrmClient : IHrmClient, ITransientDependency
    {
        public Task<List<UserInfoCacheItem>> GetUsersAsync()
        {
            return Task.FromResult(new List<UserInfoCacheItem>
            {
                new UserInfoCacheItem
                {
                    Email = "ha.nguyen@ncc.asia",
                    Name = "Ha Nguyen Ngan"
                },
                new UserInfoCacheItem
                {
                    Email = "nguyentran@ncc.asia",
                    Name = "Nhan Nguyen"
                }
            });
        }
    }
}
