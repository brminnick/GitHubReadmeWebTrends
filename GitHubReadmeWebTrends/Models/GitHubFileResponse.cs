using System;

namespace VerifyGitHubReadmeLinks
{
    public class RepositoryFile
    {
        public RepositoryFile(string name, Uri? download_url) => (FileName, DownloadUrl) = (name, download_url);

        public string FileName { get; }
        public Uri? DownloadUrl { get; }
    }
}