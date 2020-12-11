using System.Collections.Generic;
using System.Linq;

namespace GitHubReadmeWebTrends.Common
{
    public record RepositoryPullRequestResponse(Repository_RepositoryPullRequestResponse Repository);

    public record Repository_RepositoryPullRequestResponse(DefaultBranchRef_RepositoryPullRequestResponse DefaultBranchRef, PullRequestResponse PullRequests);

    public record DefaultBranchRef_RepositoryPullRequestResponse(string Name);

    public class PullRequestResponse
    {
        public PullRequestResponse(IEnumerable<PullRequest> nodes, PageInfo pageInfo) =>
            (PullRequests, PageInfo) = (nodes.ToList(), pageInfo);

        public IReadOnlyList<PullRequest> PullRequests { get; }
        public PageInfo PageInfo { get; }
    }

    public record Author(string? Login);
}
