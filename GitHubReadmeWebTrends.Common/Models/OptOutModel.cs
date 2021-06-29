using System;

namespace GitHubReadmeWebTrends.Common
{
    public record OptOutModel
    {
        public OptOutModel()
        {

        }

        public OptOutModel(string alias, bool hasOptedOut, DateTimeOffset createdAt, DateTimeOffset updatedAt)
        {
            Alias = alias;
            HasOptedOut = hasOptedOut;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }

        public string Alias { get; init; } = string.Empty;
        public bool HasOptedOut { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset UpdatedAt { get; init; }
    }
}