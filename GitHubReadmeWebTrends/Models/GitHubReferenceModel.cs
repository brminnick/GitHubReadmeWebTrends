using System;
using Newtonsoft.Json;

namespace VerifyGitHubReadmeLinks
{
    public class GitHubReferenceModel
    {
        [JsonProperty("ref")]
        public string Ref { get; set; } = string.Empty;

        [JsonProperty("node_id")]
        public string NodeId { get; set; } = string.Empty;

        [JsonProperty("url")]
        public Uri? Url { get; set; }

        [JsonProperty("object")]
        public GitHubReferenceObject? ReferenceObject { get; set; }
    }

    public class GitHubReferenceObject
    {
        public GitHubReferenceObject(string type, string sha, Uri? url) =>
            (Type, Sha, Url) = (type, sha, url);

        public static GitHubReferenceObject Empty { get; } = new GitHubReferenceObject(string.Empty, string.Empty, null);

        [JsonProperty("type")]
        public string Type { get; }

        [JsonProperty("sha")]
        public string Sha { get; }

        [JsonProperty("url")]
        public Uri? Url { get; }
    }
}
