using Newtonsoft.Json;

namespace VerifyGitHubReadmeLinks
{
    class RepositoryConnectionResponse
    {
        public RepositoryConnectionResponse(User_RepositoryConnectionResponse user) => Repository = user.Repository;

        [JsonProperty("repository")]
        public Repository Repository { get; }
    }

    class User_RepositoryConnectionResponse
    {
        public User_RepositoryConnectionResponse(string login, Repository_RepositoryConnectionResponse repository) =>
            Repository = new Repository(repository.Id, login, repository.Name, repository.DefaultBranch);

        [JsonProperty("repository")]
        public Repository Repository { get; }
    }

    class Repository_RepositoryConnectionResponse
    {
        public Repository_RepositoryConnectionResponse(string id, string name, DefaultBranchModel defaultBranchRef) =>
            (Id, Name, DefaultBranch) = (id, name, defaultBranchRef);

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("defaultBranchRef")]
        public DefaultBranchModel DefaultBranch { get; set; }
    }
}