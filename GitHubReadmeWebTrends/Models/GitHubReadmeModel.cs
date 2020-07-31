using System;
using Newtonsoft.Json;
namespace VerifyGitHubReadmeLinks
{
    public class GitHubReadmeModel
    {
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("encoding")]
        public string Encoding { get; set; } = string.Empty;

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("path")]
        public string Path { get; set; } = string.Empty;

        [JsonProperty("content")]
        public string ContentAsBase64String { get; set; } = string.Empty;

        [JsonProperty("sha")]
        public string Sha { get; set; } = string.Empty;

        [JsonProperty("url")]
        public Uri? Url { get; set; }

        [JsonProperty("git_url")]
        public Uri? GitUrl { get; set; }

        [JsonProperty("html_url")]
        public Uri? HtmlUrl { get; set; }

        [JsonProperty("download_url")]
        public Uri? DownloadUrl { get; set; }
    }
}
