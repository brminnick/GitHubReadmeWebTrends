using System;
using Newtonsoft.Json;
namespace VerifyGitHubReadmeLinks
{
    public class GitHubReadmeModel
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("encoding")]
        public string Encoding { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("content")]
        public string ContentAsBase64String { get; set; }

        [JsonProperty("sha")]
        public string Sha { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("git_url")]
        public Uri GitUrl { get; set; }

        [JsonProperty("html_url")]
        public Uri HtmlUrl { get; set; }

        [JsonProperty("download_url")]
        public Uri DownloadUrl { get; set; }
    }
}
