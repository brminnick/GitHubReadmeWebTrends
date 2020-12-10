using System;
namespace GitHubReadmeWebTrends.Common
{
    public class PullRequest
    {
        public PullRequest(DateTimeOffset createdAt, bool merged, DateTimeOffset? mergedAt, string baseRefName, Author author)
        {
            CreatedAt = createdAt;
            IsMerged = merged;
            MergedAt = mergedAt;
            BaseRefName = baseRefName;
            Author = author.Login;
        }

        public DateTimeOffset CreatedAt { get; }
        public bool IsMerged { get; }
        public DateTimeOffset? MergedAt { get; }
        public string BaseRefName { get; }
        public string Author { get; }
    }
}
