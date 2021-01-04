using Newtonsoft.Json;

namespace GitHubReadmeWebTrends.Common
{
    public class CloudAdvocateGitHubUserModel
    {
        public CloudAdvocateGitHubUserModel(in string fullName, in string gitHubUserName, in string microsoftAlias, in string microsoftTeam)
        {
            FullName = fullName;
            GitHubUserName = gitHubUserName;
            MicrosoftAlias = microsoftAlias;
            MicrosoftTeam = microsoftTeam;
        }

        [JsonProperty("fullName")]
        public string FullName { get; }

        [JsonProperty("gitHubUserName")]
        public string GitHubUserName { get; }

        [JsonProperty("microsoftAlias")]
        public string MicrosoftAlias { get; }

        [JsonProperty("microsoftTeam")]
        public string MicrosoftTeam { get; }
    }
}
