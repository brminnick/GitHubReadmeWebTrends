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

        public string FullName { get; }
        public string GitHubUserName { get; }
        public string MicrosoftAlias { get; }
        public string MicrosoftTeam { get; }
    }
}
