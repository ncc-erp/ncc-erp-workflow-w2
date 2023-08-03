using Newtonsoft.Json;

namespace W2.ExternalResources
{
    public class UserBranchInfo
    {
        [JsonProperty("fullName")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("branchCode")]
        public string BranchCode { get; set; }
    }
}
