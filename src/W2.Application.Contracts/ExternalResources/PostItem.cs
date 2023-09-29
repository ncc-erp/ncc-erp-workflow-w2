using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace W2.ExternalResources
{
    public class Title
    {
        [JsonProperty("rendered")]
        public string Rendered { get; set; }
    }

    public class PostItem
    {
        [JsonProperty("id")]
        public int id { get; set; }

        [JsonProperty("title")]
        public Title title { get; set; }

        [JsonProperty("status")]
        public string status { get; set; }

        [JsonProperty("date")]
        public string date { get; set; }

        [JsonProperty("link")]
        public string link { get; set; }
    }
}
