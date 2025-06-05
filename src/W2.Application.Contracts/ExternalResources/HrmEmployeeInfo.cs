using Newtonsoft.Json;

namespace W2.ExternalResources
{
    public class HrmEmployeeInfo
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("fullName")]
        public string FullName { get; set; }

        [JsonProperty("branchCode")]
        public string BranchCode { get; set; }

        [JsonProperty("jobPositionCode")]
        public string JobPositionCode { get; set; }

        [JsonProperty("userType")]
        public int UserType { get; set; }

        [JsonProperty("userTypeName")]
        public string UserTypeName { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("statusName")]
        public string StatusName { get; set; }

        [JsonProperty("mezonId")]
        public string MezonId { get; set; }
    }
}
