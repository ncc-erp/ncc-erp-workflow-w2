using W2.Localization;
using Volo.Abp.Application.Services;
using Elsa.Activities.Http.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace W2;

/* Inherit your application services from this class.
 */
public abstract class W2AppService : ApplicationService
{
    private readonly JsonSerializerSettings _jsonSerializerSettings;

    protected string CurrentTenantStrId => CurrentTenant?.Id?.ToString();
 
    protected W2AppService()
    {
        LocalizationResource = typeof(W2Resource);
        _jsonSerializerSettings = new JsonSerializerSettings 
        { 
            ContractResolver = new CamelCasePropertyNamesContractResolver() 
            { 
                NamingStrategy = new CamelCaseNamingStrategy(false, false) 
            } 
        };
    }

    protected HttpRequestModel GetHttpRequestModel(string method, object requestBody = null)
    {
        var requestBodyJson = requestBody == null
            ? null
            : JsonConvert.SerializeObject(requestBody, _jsonSerializerSettings);

        return new HttpRequestModel(
            null,
            method,
            null,
            null,
            null,
            Body: requestBody
        );
    }
}
