using Newtonsoft.Json;

namespace W2.ExternalResources
{
    public class OfficeInfo
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
        [JsonProperty("code")]
        public string Code { get; set; }
        [JsonProperty("headOfOfficeEmail")]
        public string HeadOfOfficeEmail { get; set; }
    }
}
