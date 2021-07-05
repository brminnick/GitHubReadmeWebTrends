using System.Text.Json.Serialization;

namespace GitHubReadmeWebTrends.Common
{
    public class UpdateFileContentModel
    {
        public UpdateFileContentModel(string message, Committer committer, string content, string sha, string branch) =>
            (CommitMessage, Committer, ContentAsBase64String, Sha, Branch) = (message, committer, content, sha, branch);

        [JsonPropertyName("branch")]
        public string Branch { get; }

        [JsonPropertyName("message")]
        public string CommitMessage { get; }

        [JsonPropertyName("committer")]
        public Committer Committer { get; }

        [JsonPropertyName("content")]
        public string ContentAsBase64String { get; }

        [JsonPropertyName("sha")]
        public string Sha { get; }
    }

    public class Committer
    {
        public Committer(string name, string email) =>
            (Name, Email) = (name, email);

        [JsonPropertyName("name")]
        public string Name { get; }

        [JsonPropertyName("email")]
        public string Email { get; }
    }
}
