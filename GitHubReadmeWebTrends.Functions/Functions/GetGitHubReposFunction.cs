using System.Collections.Generic;
using System.Threading.Tasks;
using GitHubReadmeWebTrends.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GitHubReadmeWebTrends.Functions
{
    class GetGitHubReposFunction
    {
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;

        public GetGitHubReposFunction(GitHubGraphQLApiService gitHubGraphQLApiService) => _gitHubGraphQLApiService = gitHubGraphQLApiService;

        [Function(nameof(GetGitHubReposFunction)), QueueOutput(QueueConstants.RepositoriesQueue)]
        public async Task<IReadOnlyList<RepositoryAdvocateModel>> Run([QueueTrigger(QueueConstants.AdvocatesQueue)] AdvocateModel gitHubUser, FunctionContext context)
        {
            var log = context.GetLogger<GetGitHubReposFunction>();

            log.LogInformation($"{nameof(GetGitHubReposFunction)} Started for {gitHubUser.GitHubUsername}");

            var outputData = new List<RepositoryAdvocateModel>();

            await foreach (var repositoryList in _gitHubGraphQLApiService.GetRepositories(gitHubUser.GitHubUsername).ConfigureAwait(false))
            {
                foreach (var repository in repositoryList)
                {
                }
            }

            log.LogInformation($"{nameof(GetGitHubReposFunction)} Completed");

            return outputData;
        }
    }
}
