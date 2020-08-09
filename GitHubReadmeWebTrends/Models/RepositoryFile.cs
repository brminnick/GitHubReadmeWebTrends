using System;
using Newtonsoft.Json;

namespace VerifyGitHubReadmeLinks
{
    public class RepositoryFile
    {
        public RepositoryFile(string name, Uri? download_url) => (FileName, DownloadUrl) = (name, download_url);

        [JsonProperty("name")]
        public string FileName { get; }

        [JsonProperty("download_url")]
        public Uri? DownloadUrl { get; }
    }
}