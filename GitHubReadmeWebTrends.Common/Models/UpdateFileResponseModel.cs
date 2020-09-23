using Newtonsoft.Json;

namespace GitHubReadmeWebTrends.Common
{
    public class UpdateFileResponseModel
    {
        public UpdateFileResponseModel(RepositoryFile content, CommitModel commit) =>
            (File, Commit) = (content, commit);

        [JsonProperty("content")]
        public RepositoryFile File { get; }

        [JsonProperty("commit")]
        public CommitModel Commit { get; }
    }
}
