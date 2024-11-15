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

namespace W2.Komu
{
    [Authorize]
    public class KomuService: W2AppService, IKomuService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly Configurations.KomuConfiguration _komuConfiguration;
        private readonly ILogger<KomuService> _logger;

        public KomuService(HttpClient httpClient, IConfiguration configuration, IOptions<Configurations.KomuConfiguration> komuConfigurationOptions)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _komuConfiguration = komuConfigurationOptions.Value;
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
                    await _httpClient.SendAsync(request);
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex);
                }
            }
        }
    }
}
