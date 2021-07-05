using System.Collections.Generic;
using GitHubReadmeWebTrends.Common;

namespace AzureAdvocates.Functions
{
    public record CloudAdvocateGitHubContributorModel(IReadOnlyList<RepositoryPullRequest> PullRequests, string GitHubUsername, string MicrosoftAlias, string? RedditUserName, string Team, string Name)
        : AdvocateModel(GitHubUsername, MicrosoftAlias, RedditUserName, Team, Name);
}
