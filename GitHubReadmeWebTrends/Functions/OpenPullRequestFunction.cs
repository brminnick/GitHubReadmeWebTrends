using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace VerifyGitHubReadmeLinks
{
    class OpenPullRequestFunction
    {
        readonly GitHubApiService _gitHubApiService;
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;

        public OpenPullRequestFunction(GitHubApiService gitHubApiService, GitHubGraphQLApiService gitHubGraphQLApiService) =>
            (_gitHubApiService, _gitHubGraphQLApiService) = (gitHubApiService, gitHubGraphQLApiService);

        [FunctionName(nameof(OpenPullRequestFunction))]
        public async Task Run([QueueTrigger(QueueConstants.OpenPullRequestQueue)] Repository repository, ILogger log)
        {
            try
            {
                var prefixes = repository.DefaultBranchPrefix.Split("/");

                var defaultBranchReference = await _gitHubApiService.GetDefaultBranchRefrence(repository.Owner, repository.Name, prefixes[0], prefixes[1], repository.DefaultBranchName).ConfigureAwait(false);

                var createBranchGiud = Guid.NewGuid();
                var createBranchResult = await _gitHubGraphQLApiService.CreateBranch(repository.Id, repository.DefaultBranchPrefix + "WebTrends", repository.DefaultBranchOid, createBranchGiud).ConfigureAwait(false);

                if (createBranchResult.ClientMutationId != createBranchGiud.ToString())
                    throw new Exception("Failed to Create New Branch");
            }
            catch (Exception e)
            {

            }
        }
    }
}
