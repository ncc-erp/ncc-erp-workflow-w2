using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace W2.Komu
{
    [Authorize]
    public class KomuAppService: W2AppService, IKomuAppService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly Configurations.KomuConfiguration _komuConfiguration;
        private readonly ILogger<KomuAppService> _logger;
        private readonly IRepository<W2KomuMessageLogs, Guid> _W2KomuMessageLogsRepository;

        public KomuAppService(
            HttpClient httpClient, 
            IConfiguration configuration, 
            IOptions<Configurations.KomuConfiguration> komuConfigurationOptions,
            IRepository<W2KomuMessageLogs, Guid> W2KomuMessageLogsRepository)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _komuConfiguration = komuConfigurationOptions.Value;
            _W2KomuMessageLogsRepository = W2KomuMessageLogsRepository;
        }

        [AllowAnonymous]
        public async Task<List<KomuMessageLogDto>> GetKomuMessageLogListAsync(string userName)
        {
            IQueryable<W2KomuMessageLogs> queryableLogs = await _W2KomuMessageLogsRepository.GetQueryableAsync();

            if (!string.IsNullOrEmpty(userName))
            {
                queryableLogs = queryableLogs.Where(log => log.SendTo == userName);
            }

            List<W2KomuMessageLogs> filteredLogs = await queryableLogs.ToDynamicListAsync<W2KomuMessageLogs>();

            List<KomuMessageLogDto> komuMessageLogDto = ObjectMapper.Map<List<W2KomuMessageLogs>, List<KomuMessageLogDto>>(filteredLogs);

            return komuMessageLogDto;
        }

        [RemoteService(IsEnabled = false)]
        public async Task KomuSendMessageAsync(string username, string message = "")
        {
            if(!String.IsNullOrEmpty(username))
            {
                var komuApiUrl = _komuConfiguration.ApiUrl;
                var komuXSecretKey = _komuConfiguration.XSecretKey;

                var requestData = new
                {
                    username,
                    message
                };

                var jsonContent = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, komuApiUrl + "sendMessageToUser")
                {
                    Content = jsonContent
                };

                request.Headers.Add("X-Secret-Key", komuXSecretKey);

                try
                {
                    var systemResponse = await _httpClient.SendAsync(request);
                    await _W2KomuMessageLogsRepository.InsertAsync(new W2KomuMessageLogs {
                        SendTo = username,
                        Message = message,
                        SystemResponse = systemResponse.ToString(),
                        Status = 1,
                        CreatorId = CurrentUser.Id,
                        CreationTime = DateTime.Now
                    });

                }
                catch (Exception ex)
                {
                    await _W2KomuMessageLogsRepository.InsertAsync(new W2KomuMessageLogs
                    {
                        SendTo = username,
                        Message = message,
                        SystemResponse = ex.Message,
                        Status = 0,
                        CreatorId = CurrentUser.Id,
                        CreationTime = DateTime.Now
                    });

                    _logger.LogException(ex);
                }
            }
        }
    }
}
