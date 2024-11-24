using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
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

        [RemoteService(IsEnabled = false)]
        [AllowAnonymous]
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
                    _logger.LogException(ex);
                    await _W2KomuMessageLogsRepository.InsertAsync(new W2KomuMessageLogs
                    {
                        SendTo = username,
                        Message = message,
                        SystemResponse = ex.Message,
                        Status = 0,
                        CreatorId = CurrentUser.Id,
                        CreationTime = DateTime.Now
                    });
                }
            }
        }
    }
}
