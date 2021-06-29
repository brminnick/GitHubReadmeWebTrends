using System;
using System.Text.Json.Serialization;

namespace GitHubReadmeWebTrends.Common
{
    public record PullRequest
    {
        [JsonConstructor]
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

        [JsonPropertyName("id")]
        public string Id { get; }

        [JsonPropertyName("url")]
        public Uri Uri { get; }

        [JsonPropertyName("createdAt")]
        public DateTimeOffset CreatedAt { get; }

        [JsonPropertyName("merged")]
        public bool IsMerged { get; }

        [JsonPropertyName("mergedAt")]
        public DateTimeOffset? MergedAt { get; }

        [JsonPropertyName("baseRefName")]
        public string BaseRefName { get; }

        [JsonPropertyName("author")]
        public string Author { get; }
    }

    public record RepositoryPullRequest : PullRequest
    {
        public RepositoryPullRequest(string repositoryName, string repositoryOwner, PullRequest pullRequest)
            : this(repositoryName, repositoryOwner, pullRequest.Id, pullRequest.Uri, pullRequest.CreatedAt, pullRequest.IsMerged, pullRequest.MergedAt, pullRequest.BaseRefName, pullRequest.Author)
        {

        }

        [JsonConstructor]
        public RepositoryPullRequest(string repositoryName, string repositoryOwner, string id, Uri url, DateTimeOffset createdAt, bool merged, DateTimeOffset? mergedAt, string baseRefName, string author)
            : base(id, url, createdAt, merged, mergedAt, baseRefName, author)
        {
            RepositoryName = repositoryName;
            RepositoryOwner = repositoryOwner;
        }

        [JsonPropertyName("repositoryName")]
        public string RepositoryName { get; }

        [JsonPropertyName("repositoryOwner")]
        public string RepositoryOwner { get; }
    }
}
