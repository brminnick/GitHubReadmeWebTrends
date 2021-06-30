using System.Collections.Generic;
using System.Linq;

namespace GitHubReadmeWebTrends.Common
{
    public record RepositoryPullRequestResponse(Repository_RepositoryPullRequestResponse Repository);

    public record Repository_RepositoryPullRequestResponse(DefaultBranchRef_RepositoryPullRequestResponse DefaultBranchRef, PullRequestResponse PullRequests);

    public record DefaultBranchRef_RepositoryPullRequestResponse(string Name);

    public record PullRequestResponse(IEnumerable<PullRequest> Nodes, PageInfo PageInfo);

    public record Author(string? Login);
}
