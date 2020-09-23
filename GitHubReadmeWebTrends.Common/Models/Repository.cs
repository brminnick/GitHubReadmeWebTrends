using Newtonsoft.Json;

namespace GitHubReadmeWebTrends.Common
{
    public class Repository
    {
        public Repository(string id, string owner, string name, DefaultBranchModel defaultBranch, bool isFork)
            : this(id, owner, name, defaultBranch.BranchOid, defaultBranch.Prefix, defaultBranch.Name, isFork, string.Empty)
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

        [JsonProperty("id")]
        public string Id { get; }

        [JsonProperty("owner")]
        public string Owner { get; }

        [JsonProperty("name")]
        public string Name { get; }

        [JsonProperty("isFork")]
        public bool IsFork { get; }

        [JsonProperty("defaultBranchOid")]
        public string DefaultBranchOid { get; }

        [JsonProperty("defaultBranchPrefix")]
        public string DefaultBranchPrefix { get; }

        [JsonProperty("defaultBranchName")]
        public string DefaultBranchName { get; }

        [JsonProperty("readmeText")]
        public string ReadmeText { get; }
    }
}
