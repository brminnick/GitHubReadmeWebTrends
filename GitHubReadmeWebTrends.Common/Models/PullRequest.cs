using System;
using Newtonsoft.Json;

namespace GitHubReadmeWebTrends.Common
{
    public class PullRequest
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

        [JsonProperty("id")]
        public string Id { get; }

        [JsonProperty("url")]
        public Uri Uri { get; }

        [JsonProperty("createdAt")]
        public DateTimeOffset CreatedAt { get; }

        [JsonProperty("merged")]
        public bool IsMerged { get; }

        [JsonProperty("mergedAt")]
        public DateTimeOffset? MergedAt { get; }

        [JsonProperty("baseRefName")]
        public string BaseRefName { get; }

        [JsonProperty("author")]
        public string Author { get; }
    }

    public class RepositoryPullRequest : PullRequest
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

        [JsonProperty("repositoryName")]
        public string RepositoryName { get; }

        [JsonProperty("repositoryOwner")]
        public string RepositoryOwner { get; }
    }
}
