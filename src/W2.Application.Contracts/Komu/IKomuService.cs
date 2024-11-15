using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using W2.Settings;

namespace W2.Komu
{
    public interface IKomuService : IApplicationService
    {
        Task KomuSendMessageAsync(string userName, string message);
    }
}
