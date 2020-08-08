namespace VerifyGitHubReadmeLinks
{
    class CloudAdvocateGitHubUserModel
    {
        public CloudAdvocateGitHubUserModel(in string fullName, in string userName, in string microsoftAlias) =>
            (FullName, UserName, MicrosoftAlias) = (fullName, userName, microsoftAlias);

        public string FullName { get; }
        public string UserName { get; }
        public string MicrosoftAlias { get; }
    }
}
