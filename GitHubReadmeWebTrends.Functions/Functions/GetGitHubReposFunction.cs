using System.Threading.Tasks;
using GitHubReadmeWebTrends.Common;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace GitHubReadmeWebTrends.Functions
{
    class GetGitHubReposFunction
    {
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;

        public GetGitHubReposFunction(GitHubGraphQLApiService gitHubGraphQLApiService) => _gitHubGraphQLApiService = gitHubGraphQLApiService;

        [FunctionName(nameof(GetGitHubReposFunction))]
        public async Task Run([QueueTrigger(QueueConstants.AdvocatesQueue)] CloudAdvocateGitHubUserModel gitHubUser, ILogger log,
                                [Queue(QueueConstants.RepositoriesQueue)] ICollector<(Repository, CloudAdvocateGitHubUserModel)> outputData)
        {
            log.LogInformation($"{nameof(GetGitHubReposFunction)} Started");

            await foreach (var repositoryList in _gitHubGraphQLApiService.GetRepositories(gitHubUser.GitHubUserName).ConfigureAwait(false))
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
