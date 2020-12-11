using GitHubReadmeWebTrends.Common;

namespace GitHubReadmeWebTrends.Functions
{
    class GitHubGraphQLApiPrivateRepoService : GitHubGraphQLApiService
    {
        public GitHubGraphQLApiPrivateRepoService(IGitHubGraphQLApiClient_PrivateRepoAccess gitHubGraphQLApiClient) : base(gitHubGraphQLApiClient)
        {
        }
    }
}
