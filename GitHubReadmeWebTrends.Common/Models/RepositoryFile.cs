using System;
using System.Text.Json.Serialization;

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

        [JsonPropertyName("name")]
        public string FileName { get; }

        [JsonPropertyName("sha")]
        public string Sha { get; }

        [JsonPropertyName("path")]
        public string Path { get; }

        [JsonPropertyName("download_url")]
        public Uri? DownloadUrl { get; }
    }
}