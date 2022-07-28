using Newtonsoft.Json;

namespace W2.ExternalResources
{
    public class UserInfoCacheItem
    {
        [JsonProperty("fullName")]
        public string Name { get; set; }

        [JsonProperty("emailAddress")]
        public string Email { get; set; }
    }
}
