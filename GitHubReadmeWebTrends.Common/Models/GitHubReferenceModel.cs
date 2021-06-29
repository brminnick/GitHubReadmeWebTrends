using System;

namespace GitHubReadmeWebTrends.Common
{
    public record GitHubReferenceObject(string Type, string Sha, Uri? Url)
    {
        public static GitHubReferenceObject Empty { get; } = new GitHubReferenceObject(string.Empty, string.Empty, null);
    }
}
