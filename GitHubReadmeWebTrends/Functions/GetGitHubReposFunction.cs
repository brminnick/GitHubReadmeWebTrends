using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace VerifyGitHubReadmeLinks
{
    class GetGitHubReposFunction
    {
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;

        public GetGitHubReposFunction(GitHubGraphQLApiService gitHubGraphQLApiService) => _gitHubGraphQLApiService = gitHubGraphQLApiService;

        [FunctionName(nameof(GetGitHubReposFunction))]
        public async Task Run([QueueTrigger(QueueConstants.AdvocatesQueue)] CloudAdvocateGitHubUserModel gitHubUser, ILogger log,
                                [Queue(QueueConstants.RepositoriesQueue)] ICollector<(Repository, CloudAdvocateGitHubUserModel)> gitHubUserOutput)
        {
            log.LogInformation($"{nameof(GetGitHubReposFunction)} Started");

            await foreach (var repositoryList in _gitHubGraphQLApiService.GetRepositories(gitHubUser.UserName).ConfigureAwait(false))
            {
                foreach (var repository in repositoryList)
                {
                    gitHubUserOutput.Add((new Repository(gitHubUser.UserName, repository), gitHubUser));
                }
            }

            log.LogInformation($"{nameof(GetGitHubReposFunction)} Completed");
        }
    }
}
