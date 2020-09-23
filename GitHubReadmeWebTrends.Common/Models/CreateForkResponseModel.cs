using Newtonsoft.Json;

namespace GitHubReadmeWebTrends.Common
{
    public class CreateForkResponseModel
    {
        public CreateForkResponseModel(string node_id, string name, string default_branch, Owner owner) =>
            (NodeId, Name, DefaultBranch, OwnerLogin) = (node_id, name, default_branch, owner.Login);

        [JsonProperty("node_id")]
        public string NodeId { get; }

        [JsonProperty("name")]
        public string Name { get; }

        [JsonProperty("default_branch")]
        public string DefaultBranch { get; }

        [JsonProperty("owner_login")]
        public string OwnerLogin { get; }
    }

    public class Owner
    {
        public Owner(string login) => Login = login;

        [JsonProperty("login")]
        public string Login { get; }
    }
}