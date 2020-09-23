using Newtonsoft.Json;

namespace GitHubReadmeWebTrends.Common
{
    public class DefaultBranchModel
    {
        public DefaultBranchModel(string id, string name, string prefix, Target target) =>
            (Id, Name, Prefix, BranchOid) = (id, name, prefix, target.Oid);

        [JsonProperty("id")]
        public string Id { get; }

        [JsonProperty("oid")]
        public string BranchOid { get; }

        [JsonProperty("name")]
        public string Name { get; }

        [JsonProperty("prefix")]
        public string Prefix { get; }
    }

    public class Target
    {
        public Target(string oid) => Oid = oid;

        [JsonProperty("oid")]
        public string Oid { get; }
    }
}
