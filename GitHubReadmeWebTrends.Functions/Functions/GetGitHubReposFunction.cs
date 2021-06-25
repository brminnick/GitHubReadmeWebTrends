using System.Threading.Tasks;
using GitHubReadmeWebTrends.Common;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace GitHubReadmeWebTrends.Functions
{
    public class GetGitHubReposFunction
    {
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;

        public GetGitHubReposFunction(GitHubGraphQLApiService gitHubGraphQLApiService) => _gitHubGraphQLApiService = gitHubGraphQLApiService;

        [FunctionName(nameof(GetGitHubReposFunction))]
        public async Task Run([QueueTrigger(QueueConstants.AdvocatesQueue)] AdvocateModel gitHubUser, ILogger log,
                                [Queue(QueueConstants.RepositoriesQueue)] ICollector<(Repository, AdvocateModel)> outputData)
        {
            log.LogInformation($"{nameof(GetGitHubReposFunction)} Started");

            await foreach (var repositoryList in _gitHubGraphQLApiService.GetRepositories(gitHubUser.GitHubUsername).ConfigureAwait(false))
            {
                foreach (var repository in repositoryList)
                {
                    outputData.Add((repository, gitHubUser));
                }
            }

            log.LogInformation($"{nameof(GetGitHubReposFunction)} Completed");
        }
    }
}
