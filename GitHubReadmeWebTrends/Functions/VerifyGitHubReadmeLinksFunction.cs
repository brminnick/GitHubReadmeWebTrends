using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace VerifyGitHubReadmeLinks
{
    class VerifyGitHubReadmeLinksFunction
    {
        const string _runOncePerMonth = "0 0 5 * * *";
        readonly GitHubApiService _gitHubApiService;

        public VerifyGitHubReadmeLinksFunction(GitHubApiService gitHubApiService) => _gitHubApiService = gitHubApiService;

        [FunctionName(nameof(VerifyGitHubReadmeLinksFunction))]
        public async Task<IActionResult> Run([TimerTrigger(_runOncePerMonth)] TimerInfo myTimer, ILogger log)
        {
            await foreach(var azureAdvocateGitHubModel in _gitHubApiService.GetAzureAdvocates().ConfigureAwait(false))
            {

            }

            return new OkResult();
        }
    }
}