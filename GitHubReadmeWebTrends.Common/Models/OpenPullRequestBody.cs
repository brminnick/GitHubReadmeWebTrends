using Newtonsoft.Json;

namespace GitHubReadmeWebTrends.Common
{
    public class OpenPullRequestModel
    {
        public OpenPullRequestModel(string title, string body, string head, string baseText) =>
            (Title, Body, Head, Base) = (title, body, head, baseText);

        [JsonProperty("title")]
        public string Title { get; }

        [JsonProperty("body")]
        public string Body { get; }

        [JsonProperty("head")]
        public string Head { get; }

        [JsonProperty("base")]
        public string Base { get; }

    }
}
