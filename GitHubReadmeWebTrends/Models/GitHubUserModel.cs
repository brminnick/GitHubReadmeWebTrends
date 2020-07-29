namespace VerifyGitHubReadmeLinks
{
    class GitHubUserModel
    {
        public GitHubUserModel(string fullName, string gitHubUserName) => (FullName, UserName) = (fullName, gitHubUserName);

        public string FullName { get; }
        public string UserName { get; }
    }
}
