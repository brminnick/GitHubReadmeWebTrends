using System.Collections.Generic;
using System.Threading.Tasks;
using GitHubReadmeWebTrends.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GitHubReadmeWebTrends.Functions
{
    public class GetGitHubReposFunction
    {
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;

        public GetGitHubReposFunction(GitHubGraphQLApiService gitHubGraphQLApiService) => _gitHubGraphQLApiService = gitHubGraphQLApiService;

        [Function(nameof(GetGitHubReposFunction)), QueueOutput(QueueConstants.RepositoriesQueue)]
        public async Task<IReadOnlyList<(Repository, AdvocateModel)>> Run([QueueTrigger(QueueConstants.AdvocatesQueue)] AdvocateModel gitHubUser, FunctionContext context)
        {
            var log = context.GetLogger<GetGitHubReposFunction>();

            log.LogInformation($"{nameof(GetGitHubReposFunction)} Started");

            var outputData = new List<(Repository, AdvocateModel)>();

            await foreach (var repositoryList in _gitHubGraphQLApiService.GetRepositories(gitHubUser.GitHubUsername).ConfigureAwait(false))
            {
                foreach (var repository in repositoryList)
                {
                    outputData.Add((repository, gitHubUser));
                }
            }

            log.LogInformation($"{nameof(GetGitHubReposFunction)} Completed");

            return outputData;
        }
    }
}
