using Newtonsoft.Json;

namespace VerifyGitHubReadmeLinks
{
    public class PageInfo
    {
        public PageInfo(string endCursor, bool hasNextPage, bool hasPreviousPage,string startCursor) =>
            (EndCursor, HasNextPage, HasPreviousPage, StartCursor) = (endCursor, hasNextPage, hasPreviousPage, startCursor);

        [JsonProperty("endCursor")]
        public string EndCursor { get; }

        [JsonProperty("hasNextPage")]
        public bool HasNextPage { get; }

        [JsonProperty("hasPreviousPage")]
        public bool HasPreviousPage { get; }

        [JsonProperty("startCursor")]
        public string StartCursor { get; }
    }
}
