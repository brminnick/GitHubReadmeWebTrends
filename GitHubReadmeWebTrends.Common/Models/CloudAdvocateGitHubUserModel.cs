namespace GitHubReadmeWebTrends.Common
{
    public class CloudAdvocateGitHubUserModel
    {
        public CloudAdvocateGitHubUserModel(in string fullName, in string gitHubUserName, in string microsoftAlias) =>
            (FullName, GitHubUserName, MicrosoftAlias) = (fullName, gitHubUserName, microsoftAlias);

        public string FullName { get; }
        public string GitHubUserName { get; }
        public string MicrosoftAlias { get; }
    }
}
