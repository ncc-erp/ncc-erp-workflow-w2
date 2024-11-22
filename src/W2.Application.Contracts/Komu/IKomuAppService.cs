using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;


namespace W2.Komu
{
    public interface IKomuAppService : IApplicationService
    {
        Task KomuSendMessageAsync(string userName, string message);
        Task<List<KomuMessageLogDto>> GetKomuMessageLogListAsync(string userName);
    }
}
