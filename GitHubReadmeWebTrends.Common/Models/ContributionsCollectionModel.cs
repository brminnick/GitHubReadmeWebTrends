using System.Collections.Generic;
using System.Linq;

namespace GitHubReadmeWebTrends.Common
{
    public record ContributionsCollectionModel(int TotalIssueContributions,
                                                int TotalCommitContributions,
                                                int TotalPullRequestContributions,
                                                int TotalPullRequestReviewContributions,
                                                IEnumerable<ContributionsByRepository> CommitContributionsByRepository,
                                                IEnumerable<ContributionsByRepository> IssueContributionsByRepository,
                                                IEnumerable<ContributionsByRepository> PullRequestContributionsByRepository,
                                                IEnumerable<ContributionsByRepository> PullRequestReviewContributionsByRepository)
    {

        public int TotalContributions => TotalIssueContributions + TotalCommitContributions + TotalPullRequestContributions + TotalPullRequestReviewContributions;
    }

    public record ContributionsResponse(User_ContributionsResponse User);

    public record User_ContributionsResponse(ContributionsCollectionModel ContributionsCollection);

    public record ContributionsByRepository(Repository_ContributionsResponse Repository);

    public record Repository_ContributionsResponse(string Name);
}