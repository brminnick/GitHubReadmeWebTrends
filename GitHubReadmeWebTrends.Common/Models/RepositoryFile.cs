using System;
using Newtonsoft.Json;

namespace GitHubReadmeWebTrends.Common
{
    public class RepositoryFile
    {
        public RepositoryFile(string name, string sha, string path, Uri? download_url)
        {
            FileName = name;
            Sha = sha;
            Path = path;
            DownloadUrl = download_url;
        }

        [JsonProperty("name")]
        public string FileName { get; }

        [JsonProperty("sha")]
        public string Sha { get; }

        [JsonProperty("path")]
        public string Path { get; }

        [JsonProperty("download_url")]
        public Uri? DownloadUrl { get; }
    }
}