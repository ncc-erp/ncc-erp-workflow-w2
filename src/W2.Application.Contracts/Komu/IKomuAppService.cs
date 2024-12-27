using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using W2.Tasks;


namespace W2.Komu
{
    public interface IKomuAppService : IApplicationService
    {
        Task KomuSendMessageAsync(string userName, Guid creatorId, string message);
        Task<List<KomuMessageLogDto>> GetKomuMessageLogListAsync(string userName, string fromTime, string toTime);
        Task KomuSendTaskAssignAsync(Guid creatorId, string wfId);
    }
}
