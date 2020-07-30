using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace VerifyGitHubReadmeLinks
{
    class GetAdvocatesFunction
    {
        const string _runOncePerMonth = "0 0 5 * * *";

        readonly GitHubApiService _gitHubApiService;

        public GetAdvocatesFunction(GitHubApiService gitHubApiService) => _gitHubApiService = gitHubApiService;

        [FunctionName(nameof(GetAdvocatesFunction))]
        public async Task Run([TimerTrigger(_runOncePerMonth)] TimerInfo myTimer, ILogger log,
            [Queue(QueueConstants.GetAdvocatesQueue)] ICollector<GitHubUserModel> advocateModels)
        {
            log.LogInformation($"{nameof(GetAdvocatesFunction)} Started");

            await foreach(var gitHubUser in _gitHubApiService.GetAzureAdvocates().ConfigureAwait(false))
            {
                advocateModels.Add(gitHubUser);
            }

            log.LogInformation($"Completed");
        }
    }
}