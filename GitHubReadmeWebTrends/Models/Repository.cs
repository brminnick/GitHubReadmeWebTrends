namespace VerifyGitHubReadmeLinks
{
    class Repository
    {
        public Repository(string owner, string name, string readmeText = "") => (Owner, Name, ReadmeText) = (owner, name, readmeText);

        public string Owner { get; }
        public string Name { get; }
        public string ReadmeText { get; }
    }
}
