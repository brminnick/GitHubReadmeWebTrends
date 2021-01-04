using System.Collections.Generic;
using System.Linq;
using GitHubReadmeWebTrends.Common;
using Newtonsoft.Json;

namespace AzureAdvocates.Functions
{
    class CloudAdvocateGitHubContributorModel : CloudAdvocateGitHubUserModel
    {
        public CloudAdvocateGitHubContributorModel(IEnumerable<RepositoryPullRequest> pullRequests, CloudAdvocateGitHubUserModel cloudAdvocateGitHubUserModel)
            : this(pullRequests, cloudAdvocateGitHubUserModel.FullName, cloudAdvocateGitHubUserModel.GitHubUserName, cloudAdvocateGitHubUserModel.MicrosoftAlias, cloudAdvocateGitHubUserModel.MicrosoftTeam)
        {

        }

        [JsonConstructor]
        public CloudAdvocateGitHubContributorModel(IEnumerable<RepositoryPullRequest> pullRequests, string fullName, string gitHubUserName, string microsoftAlias, string microsoftTeam)
            : base(fullName, gitHubUserName, microsoftAlias, microsoftTeam)
        {
            PullRequests = pullRequests.ToList(); ;
        }

        [JsonProperty("pullRequests")]
        public IReadOnlyList<RepositoryPullRequest> PullRequests { get; }
    }
}
