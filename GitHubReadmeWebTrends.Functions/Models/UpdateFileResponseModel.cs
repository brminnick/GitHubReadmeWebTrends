using Newtonsoft.Json;

namespace VerifyGitHubReadmeLinks.Functions
{
     class UpdateFileResponseModel
    {
        public UpdateFileResponseModel(RepositoryFile content, CommitModel commit) =>
            (File, Commit) = (content, commit);

        [JsonProperty("content")]
        public RepositoryFile File { get; }

        [JsonProperty("commit")]
        public CommitModel Commit { get; }
    }
}
