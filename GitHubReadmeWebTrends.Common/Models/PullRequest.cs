using System;
namespace GitHubReadmeWebTrends.Common
{
    public class PullRequest
    {
        [Newtonsoft.Json.JsonConstructor]
        public PullRequest(string id, Uri url, DateTimeOffset createdAt, bool merged, DateTimeOffset? mergedAt, string baseRefName, Author author)
            : this(id, url, createdAt, merged, mergedAt, baseRefName, author?.Login ?? string.Empty)
        {

        }

        public PullRequest(string id, Uri uri, DateTimeOffset createdAt, bool merged, DateTimeOffset? mergedAt, string baseRefName, string author)
        {
            Id = id;
            Uri = uri;
            CreatedAt = createdAt;
            IsMerged = merged;
            MergedAt = mergedAt;
            BaseRefName = baseRefName;
            Author = author;
        }

        public string Id { get; }
        public Uri Uri { get; }
        public DateTimeOffset CreatedAt { get; }
        public bool IsMerged { get; }
        public DateTimeOffset? MergedAt { get; }
        public string BaseRefName { get; }
        public string Author { get; }
    }

    public class RepositoryPullRequest : PullRequest
    {
        public RepositoryPullRequest(string repositoryName, string repositoryOwner, PullRequest pullRequest)
            : base(pullRequest.Id, pullRequest.Uri, pullRequest.CreatedAt, pullRequest.IsMerged, pullRequest.MergedAt, pullRequest.BaseRefName, pullRequest.Author)
        {
            RepositoryName = repositoryName;
            RepositoryOwner = repositoryOwner;
        }

        public string RepositoryName { get; }
        public string RepositoryOwner { get; }
    }
}
