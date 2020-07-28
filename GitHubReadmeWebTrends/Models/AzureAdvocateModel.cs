namespace VerifyGitHubReadmeLinks
{
    class AzureAdvocateModel
    {
        public AzureAdvocateModel(string fullName, string gitHubUserName) => (FullName, GitHubUserName) = (fullName, gitHubUserName);

        public string FullName { get; }
        public string GitHubUserName { get; }
    }
}
