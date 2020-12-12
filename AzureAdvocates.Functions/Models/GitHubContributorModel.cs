using System.Collections.Generic;
using System.Linq;
using GitHubReadmeWebTrends.Common;

namespace GitHubReadmeWebTrends.Functions
{
    class GitHubContributorModel : CloudAdvocateGitHubUserModel
    {
        public GitHubContributorModel(in IEnumerable<RepositoryPullRequest> pullReuests, CloudAdvocateGitHubUserModel cloudAdvocateGitHubUserModel)
            : this(pullReuests, cloudAdvocateGitHubUserModel.FullName, cloudAdvocateGitHubUserModel.GitHubUserName, cloudAdvocateGitHubUserModel.MicrosoftAlias, cloudAdvocateGitHubUserModel.MicrosoftTeam)
        {

        }

        public GitHubContributorModel(in IEnumerable<RepositoryPullRequest> pullReuests, in string fullName, in string gitHubUserName, in string microsoftAlias, in string microsoftTeam)
            : base(fullName, gitHubUserName, microsoftAlias, microsoftTeam)
        {
            PullRequests = pullReuests.ToList();
        }

        public IReadOnlyList<RepositoryPullRequest> PullRequests { get; }
    }
}
