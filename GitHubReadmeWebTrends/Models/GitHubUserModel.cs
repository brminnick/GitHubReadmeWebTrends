namespace VerifyGitHubReadmeLinks
{
    class GitHubUserModel
    {
        public GitHubUserModel(string fullName, string userName) => (FullName, UserName) = (fullName, userName);

        public string FullName { get; }
        public string UserName { get; }
    }
}
