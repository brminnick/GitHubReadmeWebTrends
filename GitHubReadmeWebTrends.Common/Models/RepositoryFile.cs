using System;

namespace GitHubReadmeWebTrends.Common
{
    public record RepositoryFile(string Name, string Sha, string Path, Uri? Download_Url);
}