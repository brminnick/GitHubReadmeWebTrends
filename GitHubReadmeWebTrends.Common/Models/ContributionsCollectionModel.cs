using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace GitHubReadmeWebTrends.Common
{
    public class ContributionsCollectionModel
    {
        public ContributionsCollectionModel(int totalIssueContributions,
                                            int totalCommitContributions,
                                            int totalPullRequestContributions,
                                            int totalPullRequestReviewContributions,
                                            IEnumerable<ContributionsByRepository> commitContributionsByRepository,
                                            IEnumerable<ContributionsByRepository> issueContributionsByRepository,
                                            IEnumerable<ContributionsByRepository> pullRequestContributionsByRepository,
                                            IEnumerable<ContributionsByRepository> pullRequestReviewContributionsByRepository)
        {
            TotalIssueContributions = totalIssueContributions;
            TotalCommitContributions = totalCommitContributions;
            TotalPullRequestContributions = totalPullRequestContributions;
            TotalPullRequestReviewContributions = totalPullRequestReviewContributions;

            CommitContributionsRepositories = commitContributionsByRepository.Select(x => x.Repository.Name).ToList();
            IssueContributionsRepositories = issueContributionsByRepository.Select(x => x.Repository.Name).ToList();
            PullRequestContributionsRepositories = pullRequestContributionsByRepository.Select(x => x.Repository.Name).ToList();
            PullRequestReviewContributionsRepositories = pullRequestReviewContributionsByRepository.Select(x => x.Repository.Name).ToList();
        }

        public int TotalIssueContributions { get; }
        public int TotalCommitContributions { get; }
        public int TotalRepositoryContributions { get; }
        public int TotalPullRequestContributions { get; }
        public int TotalPullRequestReviewContributions { get; }

        public IReadOnlyList<string> CommitContributionsRepositories { get; }
        public IReadOnlyList<string> IssueContributionsRepositories { get; }
        public IReadOnlyList<string> PullRequestContributionsRepositories { get; }
        public IReadOnlyList<string> PullRequestReviewContributionsRepositories { get; }
    }

    public record ContributionsResponse(User_ContributionsResponse User);

    public record User_ContributionsResponse(ContributionsCollectionModel ContributionsCollection);

    public record ContributionsByRepository(Repository_ContributionsResponse Repository);

    public record Repository_ContributionsResponse(string Name);
}

namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class IsExternalInit { }
}