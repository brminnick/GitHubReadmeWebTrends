using Newtonsoft.Json;

namespace VerifyGitHubReadmeLinks
{
    class Viewer
    {
        public Viewer(string name, string login, string email) =>
            (Name, Login, Email) = (name, login, email);

        [JsonProperty("name")]
        public string Name { get; }

        [JsonProperty("login")]
        public string Login { get; }

        [JsonProperty("email")]
        public string Email { get; }
    }

    class GitHubViewerResponse
    {
        public GitHubViewerResponse(Viewer viewer) => Viewer = viewer;

        public Viewer Viewer { get; }
    }
}
