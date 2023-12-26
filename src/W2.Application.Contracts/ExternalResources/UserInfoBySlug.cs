using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace W2.ExternalResources
{
    public class UserInfoBySlug
    {
        [JsonProperty("id")]
        public int id { get; set; }

        [JsonProperty("slug")]
        public string slug { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }
    }
}
