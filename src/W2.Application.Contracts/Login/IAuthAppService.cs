using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using W2.ExternalResources;

namespace W2.Login
{
    public interface IAuthAppService
    {
        Task<AuthUser> LoginAccount(AuthDto authDto);
        
        UserInfo CurrentUser();

        Task<AuthUser> LoginMezonByHash(AuthMezonByHashDto authMezonByHashDto);
    }
}
