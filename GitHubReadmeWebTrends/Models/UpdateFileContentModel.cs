using Newtonsoft.Json;

namespace VerifyGitHubReadmeLinks
{
    class UpdateFileContentModel
    {
        public UpdateFileContentModel(string message, Committer committer, string content, string sha) =>
            (CommitMessage, Committer, ContentAsBase64String, Sha) = (message, committer, content, sha);

        [JsonProperty("message")]
        public string CommitMessage { get; }

        [JsonProperty("committer")]
        public Committer Committer { get; }

        [JsonProperty("content")]
        public string ContentAsBase64String { get; }

        [JsonProperty("sha")]
        public string Sha { get; }
    }

    public class Committer
    {
        public Committer(string name, string email) =>
            (Name, Email) = (name, email);

        [JsonProperty("name")]
        public string Name { get; }

        [JsonProperty("email")]
        public string Email { get; }
    }
}
