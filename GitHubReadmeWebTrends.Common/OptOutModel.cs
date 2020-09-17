using System;

namespace GitHubReadmeWebTrends.Common
{
    public class OptOutModel : IOptOutModel
    {
        public OptOutModel(Guid id, string alias, bool hasOptedOut, DateTimeOffset createdAt, DateTimeOffset updatedAt)
        {
            Id = id;
            Alias = alias;
            HasOptedOut = hasOptedOut;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }

        public Guid Id { get; }
        public string Alias { get; }
        public bool HasOptedOut { get; }
        public DateTimeOffset CreatedAt { get; }
        public DateTimeOffset UpdatedAt { get; }
    }

    interface IOptOutModel
    {
        Guid Id { get; }
        string Alias { get; }
        bool HasOptedOut { get; }
        DateTimeOffset CreatedAt { get; }
        DateTimeOffset UpdatedAt { get; }
    }
}