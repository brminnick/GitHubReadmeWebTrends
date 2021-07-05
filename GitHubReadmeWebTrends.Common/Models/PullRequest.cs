using System;

namespace GitHubReadmeWebTrends.Common
{
    public record PullRequest(string Id, Uri Url, DateTimeOffset CreatedAt, bool Merged, DateTimeOffset? MergedAt, string BaseRefName, Author? Author);

    public record RepositoryPullRequest(string RepositoryName, string RepositoryOwner, string Id, Uri Url, DateTimeOffset CreatedAt, bool Merged, DateTimeOffset? MergedAt, string BaseRefName, Author? Author)
        : PullRequest(Id, Url, CreatedAt, Merged, MergedAt, BaseRefName, Author);
}
