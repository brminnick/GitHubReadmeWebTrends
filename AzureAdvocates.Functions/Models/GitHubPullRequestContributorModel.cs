using System.Collections.Generic;
using System.Linq;
using GitHubReadmeWebTrends.Common;
using Newtonsoft.Json;

namespace AzureAdvocates.Functions
{
    record CloudAdvocateGitHubContributorModel : AdvocateModel
    {
        public CloudAdvocateGitHubContributorModel(IEnumerable<RepositoryPullRequest> pullRequests, AdvocateModel advocateModel)
            : this(pullRequests, advocateModel.Name, advocateModel.GitHubUsername, advocateModel.MicrosoftAlias, advocateModel.Team, advocateModel.RedditUserName)
        {

        }

        [JsonConstructor]
        public CloudAdvocateGitHubContributorModel(IEnumerable<RepositoryPullRequest> pullRequests, string fullName, string gitHubUserName, string microsoftAlias, string microsoftTeam, string? redditUserName)
            : base(gitHubUserName, microsoftAlias, redditUserName, microsoftTeam, fullName)
        {
            PullRequests = pullRequests.ToList(); ;
        }

        [JsonProperty("pullRequests")]
        public IReadOnlyList<RepositoryPullRequest> PullRequests { get; }
    }
}
