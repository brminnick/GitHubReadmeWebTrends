using System.Text.Json.Serialization;

namespace GitHubReadmeWebTrends.Common
{
    public class Repository
    {
        public Repository(string id, string owner, string name, DefaultBranchModel defaultBranch, bool isFork)
            : this(id, owner, name, defaultBranch.Target.Oid, defaultBranch.Prefix, defaultBranch.Name, isFork, string.Empty)
        {

        }

        [JsonConstructor]
        public Repository(string id, string owner, string name, string defaultBranchOid, string defaultBranchPrefix, string defaultBranchName, bool isFork, string readmeText)
        {
            Id = id;
            Owner = owner;
            Name = name;
            IsFork = isFork;
            DefaultBranchOid = defaultBranchOid;
            DefaultBranchPrefix = defaultBranchPrefix;
            DefaultBranchName = defaultBranchName;
            ReadmeText = readmeText;
        }

        [JsonPropertyName("id")]
        public string Id { get; }

        [JsonPropertyName("owner")]
        public string Owner { get; }

        [JsonPropertyName("name")]
        public string Name { get; }

        [JsonPropertyName("isFork")]
        public bool IsFork { get; }

        [JsonPropertyName("defaultBranchOid")]
        public string DefaultBranchOid { get; }

        [JsonPropertyName("defaultBranchPrefix")]
        public string DefaultBranchPrefix { get; }

        [JsonPropertyName("defaultBranchName")]
        public string DefaultBranchName { get; }

        [JsonPropertyName("readmeText")]
        public string ReadmeText { get; }
    }
}
