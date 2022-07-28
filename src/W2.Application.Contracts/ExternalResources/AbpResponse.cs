using Newtonsoft.Json;
using System.Collections.Generic;

namespace W2.ExternalResources
{
    public class AbpResponse<T>
    {
        [JsonProperty("result")]
        public List<T> Result { get; set; }
    }
}
