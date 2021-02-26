using Newtonsoft.Json;

namespace GitHubReadmeWebTrends.Common
{
    public class CloudAdvocateGitHubUserModel
    {
        public CloudAdvocateGitHubUserModel(string fullName, string gitHubUserName, string microsoftAlias, string team)
        {
            FullName = fullName;
            GitHubUserName = gitHubUserName;
            MicrosoftAlias = microsoftAlias;
            MicrosoftTeam = team;
        }

        [JsonProperty("fullName")]
        public string FullName { get; }

        [JsonProperty("gitHubUserName")]
        public string GitHubUserName { get; }

        [JsonProperty("microsoftAlias")]
        public string MicrosoftAlias { get; }

        [JsonProperty("team")]
        public string MicrosoftTeam { get; }
    }
}
