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
            var forkedRepository = await ForkRepository(repository).ConfigureAwait(false);
            await CreateNewBranch(forkedRepository, "AddWebTrends").ConfigureAwait(false);
        }

        async Task<Repository> ForkRepository(Repository repository)
        {
            var createForkResponse = await _gitHubApiService.CreateFork(repository.Owner, repository.Name).ConfigureAwait(false);

            var forkedRepositoryResponse = await _gitHubGraphQLApiService.GetRepository(createForkResponse.OwnerLogin, createForkResponse.Name).ConfigureAwait(false);

            return forkedRepositoryResponse.Repository;
        }

        async Task CreateNewBranch(Repository repository, string branchName)
        {
            var createBranchGiud = Guid.NewGuid();
            var createBranchResult = await _gitHubGraphQLApiService.CreateBranch(repository.Id, repository.DefaultBranchPrefix + branchName, repository.DefaultBranchOid, createBranchGiud).ConfigureAwait(false);

            if (createBranchResult.Result.ClientMutationId != createBranchGiud.ToString())
                throw new Exception("Failed to Create New Branch: \"AddWebTrends\"");
        }
    }
}
