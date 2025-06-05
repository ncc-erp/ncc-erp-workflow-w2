using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using W2.Identity;

namespace W2.Users
{
    public interface IUserAppService : IApplicationService
    {
        Task<PagedResultDto<UserDto>> GetListAsync(ListUsersInput input);
        Task<UserRolesDto> GetUserRolesAsync(Guid userId);
        Task<UserPermissionsDto> GetUserPermissionsAsync(Guid userId);
        Task UpdateUserAsync(Guid userId, UpdateUserInput input);
        Task SyncHrmUsers();
    }
}
