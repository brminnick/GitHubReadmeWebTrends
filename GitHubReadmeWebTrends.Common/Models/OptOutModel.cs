using System;

namespace GitHubReadmeWebTrends.Common
{
    public class OptOutModel : IOptOutModel
    {
        public OptOutModel(string alias, bool hasOptedOut, DateTimeOffset createdAt, DateTimeOffset updatedAt)
        {
            Alias = alias;
            HasOptedOut = hasOptedOut;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }

        public string Alias { get; }
        public bool HasOptedOut { get; }
        public DateTimeOffset CreatedAt { get; }
        public DateTimeOffset UpdatedAt { get; }
    }

    public interface IOptOutModel
    {
        string Alias { get; }
        bool HasOptedOut { get; }
        DateTimeOffset CreatedAt { get; }
        DateTimeOffset UpdatedAt { get; }
    }
}