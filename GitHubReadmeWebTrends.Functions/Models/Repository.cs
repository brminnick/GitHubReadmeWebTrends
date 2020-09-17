using Newtonsoft.Json;

namespace VerifyGitHubReadmeLinks.Functions
{
    class Repository
    {
        public Repository(string id, string owner, string name, DefaultBranchModel defaultBranch)
            : this(id, owner, name, defaultBranch.BranchOid, defaultBranch.Prefix, defaultBranch.Name, string.Empty)
        {

        }

        [JsonConstructor]
        public Repository(string id, string owner, string name, string defaultBranchOid, string defaultBranchPrefix, string defaultBranchName, string readmeText)
        {
            Id = id;
            Owner = owner;
            Name = name;
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
