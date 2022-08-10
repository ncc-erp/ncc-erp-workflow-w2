using Newtonsoft.Json;
using System.Collections.Generic;

namespace W2.ExternalResources
{
    public class ProjectItem
    {
        [JsonProperty("projectCode")]
        public string Code { get; set; }

        [JsonProperty("projectName")]
        public string Name { get; set; }
        
        [JsonProperty("pMs")]
        public IEnumerable<ProjectManager> PM { get; set; }
    }

    public class ProjectManager
    {
        [JsonProperty("emailAddress")]
        public string EmailAddress { get; set; }
    }
}
