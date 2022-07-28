using Newtonsoft.Json;

namespace W2.ExternalResources
{
    public class ProjectItem
    {
        [JsonProperty("projectCode")]
        public string Code { get; set; }

        [JsonProperty("projectName")]
        public string Name { get; set; }
    }
}
