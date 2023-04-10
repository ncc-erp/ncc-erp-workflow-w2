using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Caching;

namespace W2.ExternalResources
{
    public class ExternalResourceAppService : W2AppService, IExternalResourceAppService
    {
        private readonly IDistributedCache<AllUserInfoCacheItem> _userInfoCache;
        private readonly IProjectClientApi _projectClient;
        private readonly ITimesheetClientApi _timesheetClient;
        private readonly IHrmClientApi _hrmClient;
        private readonly List<OfficeInfo> listOfOffices = new List<OfficeInfo>
            {
                new OfficeInfo
                {
                    Code = "HN1",
                    DisplayName = "Hà Nội 1",
                    HeadOfOfficeEmail = "tung.nguyen@ncc.asia"
                },
                new OfficeInfo
                {
                    Code = "HN2",
                    DisplayName = "Hà Nội 2",
                    HeadOfOfficeEmail = "hieu.dohoang@ncc.asia"
                },
                new OfficeInfo
                {
                    Code = "HN3",
                    DisplayName = "Hà Nội 3",
                    HeadOfOfficeEmail = "quan.nguyenminh@ncc.asia"
                },
                new OfficeInfo
                {
                    Code = "ĐN",
                    DisplayName = "Đà Nẵng",
                    HeadOfOfficeEmail = "thien.dang@ncc.asia"
                },
                new OfficeInfo
                {
                    Code = "V",
                    DisplayName = "Vinh",
                    HeadOfOfficeEmail = "dai.trinhduc@ncc.asia"
                },
                new OfficeInfo
                {
                    Code = "SG1",
                    DisplayName = "Sài Gòn 1",
                    HeadOfOfficeEmail = "duong.nguyen@ncc.asia"
                },
                new OfficeInfo
                {
                    Code = "SG2",
                    DisplayName = "Sài Gòn 2",
                    HeadOfOfficeEmail = "linh.nguyen@ncc.asia"
                },
                new OfficeInfo
                {
                    Code = "QN",
                    DisplayName = "Quy Nhơn",
                    HeadOfOfficeEmail = "phi.lekim@ncc.asia"
                }
            };

        public ExternalResourceAppService(
            IDistributedCache<AllUserInfoCacheItem> userInfoCache,
            IHrmClientApi hrmClient,
            IProjectClientApi projectClient,
            ITimesheetClientApi timesheetClient)
        {
            _userInfoCache = userInfoCache;
            _projectClient = projectClient;
            _timesheetClient = timesheetClient;
            _hrmClient = hrmClient;
        }


        public async Task<List<UserInfoCacheItem>> GetAllUsersInfoAsync()
        {
            return await _userInfoCache.GetOrAddAsync(
                AllUserInfoCacheItem.CacheKey,
                async () => await GetAllUsersInfoFromApiAsync()
            );
        }

        public async Task<List<TimesheetProjectItem>> GetCurrentUserProjectsAsync()
        {
            var email = CurrentUser.Email;
            return await GetUserProjectsFromApiAsync(email);
        }

        public async Task RefreshAllUsersInfoAsync()
        {
            await _userInfoCache.RefreshAsync(AllUserInfoCacheItem.CacheKey);
        }

        public async Task<List<TimesheetProjectItem>> GetUserProjectsWithRolePMFromApiAsync()
        {
            var response = await _timesheetClient.GetUserProjectAsync(CurrentUser.Email);
            var projects = response.Result != null ? response.Result
                .Where(x => x.PM.Any(p => p.EmailAddress == CurrentUser.Email))
                .OrderBy(x => x.Name)
                .ThenBy(x => x.Code)
                .ToList() : new List<TimesheetProjectItem>();
            return projects;
        }

        public async Task<List<OfficeInfo>> GetListOfOfficeAsync()
        {
            return await Task.FromResult(this.listOfOffices);
        }

        private async Task<AllUserInfoCacheItem> GetAllUsersInfoFromApiAsync()
        {
            var response = await _projectClient.GetUsersAsync();
            var users = response.Result != null ? response.Result
                .OrderBy(x => x.Name)
                .ThenBy(x => x.Email)
                .ToList() : new List<UserInfoCacheItem>();

            return new AllUserInfoCacheItem(users);
        }

        private async Task<List<TimesheetProjectItem>> GetUserProjectsFromApiAsync(string email)
        {
            var response = await _timesheetClient.GetUserProjectAsync(email);
            var projects = response.Result != null ? response.Result
                .OrderBy(x => x.Name)
                .ThenBy(x => x.Code)
                .ToList() : new List<TimesheetProjectItem>();

            return projects;
        }

        public async Task<OfficeInfo> GetUserBranchInfoAsync(string email)
        {
            var response = await _timesheetClient.GetUserInfoByEmailAsync(email);
            var office = this.listOfOffices.FirstOrDefault(l => l.Code == response.Result.FirstOrDefault()?.Branch);

            return office;
        }
    }
}
