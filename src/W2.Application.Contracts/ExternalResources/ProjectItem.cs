using Newtonsoft.Json;
using System.Collections.Generic;

namespace W2.ExternalResources
{
    public class ReleaseContent
    {
        public string url { get; set; }
        public string assets_url { get; set; }
        public string upload_url { get; set; }
        public string html_url { get; set; }
        public int? id { get; set; }
        public ReleaseAuthor author { get; set; }
        public string node_id { get; set; }
        public string tag_name { get; set; }
        public string target_commitish { get; set; }
        public string name { get; set; }
        public bool? draft { get; set; }
        public bool? prerelease { get; set; }
        public string created_at { get; set; }  // ISO Date string
        public string published_at { get; set; }  // ISO Date string
        public string tarball_url { get; set; }
        public string zipball_url { get; set; }
        public string body { get; set; }
        public int? mentions_count { get; set; }
    }

    public class ReleaseAuthor
    {
        public string login { get; set; }
        public int? id { get; set; }
        public string node_id { get; set; }
        public string avatar_url { get; set; }
        public string gravatar_id { get; set; }
        public string url { get; set; }
        public string html_url { get; set; }
        public string followers_url { get; set; }
        public string following_url { get; set; }
        public string gists_url { get; set; }
        public string starred_url { get; set; }
        public string subscriptions_url { get; set; }
        public string organizations_url { get; set; }
        public string repos_url { get; set; }
        public string events_url { get; set; }
        public string received_events_url { get; set; }
        public string type { get; set; }
        public string user_view_type { get; set; }
        public bool? site_admin { get; set; }
    }


    public class ProjectItemBase
    {
        [JsonProperty("projectCode")]
        public string Code { get; set; }

        [JsonProperty("projectName")]
        public string Name { get; set; }
    }

    public class TimesheetProjectItem : ProjectItemBase
    {
        [JsonProperty("pMs")]
        public IEnumerable<ProjectManager> PM { get; set; }

        public TimesheetProjectItem()
        {
            Code = string.Empty;
            Name = string.Empty;
            PM = new List<ProjectManager>();
        }
    }

    public class ProjectProjectItem : ProjectItemBase
    {
        [JsonProperty("pm")]
        public ProjectManager PM { get; set; }
    }

    public class ProjectManager
    {
        [JsonProperty("fullName")]
        public string FullName { get; set; }
        [JsonProperty("emailAddress")]
        public string EmailAddress { get; set; }
    }
}
