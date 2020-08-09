using System;
using Newtonsoft.Json;

namespace VerifyGitHubReadmeLinks
{
    class CommitModel
    {
        public CommitModel(string sha, string node_id, Uri url, Uri html_url, string message)
        {
            Sha = sha;
            NodeId = node_id;
            Url = url;
            HtmlUrl = html_url;
            Message = message;
        }

        [JsonProperty("sha")]
        public string Sha { get; }

        [JsonProperty("node_id")]
        public string NodeId { get; }

        [JsonProperty("url")]
        public Uri Url { get; }

        [JsonProperty("html_url")]
        public Uri HtmlUrl { get; }

        [JsonProperty("message")]
        public string Message { get; }
    }
}
