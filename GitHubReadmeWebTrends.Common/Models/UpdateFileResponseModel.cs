using System.Text.Json.Serialization;

namespace GitHubReadmeWebTrends.Common
{
    public class UpdateFileResponseModel
    {
        public UpdateFileResponseModel(RepositoryFile content, CommitModel commit) =>
            (File, Commit) = (content, commit);

        [JsonPropertyName("content")]
        public RepositoryFile File { get; }

        [JsonPropertyName("commit")]
        public CommitModel Commit { get; }
    }
}
