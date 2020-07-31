using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace VerifyGitHubReadmeLinks
{
    class GetReadmeFunction
    {
        readonly HttpClient _httpClient;
        readonly GitHubApiService _gitHubApiService;

        public GetReadmeFunction(GitHubApiService gitHubApiService, IHttpClientFactory httpClientFactory)
        {
            _gitHubApiService = gitHubApiService;
            _httpClient = httpClientFactory.CreateClient();
        }

        [FunctionName(nameof(GetReadmeFunction))]
        [return: Queue(QueueConstants.VerifyWebTrendsQueue)]
        public async Task<Repository> Run([QueueTrigger(QueueConstants.RepositoriesQueue)] Repository repository, ILogger log)
        {
            log.LogInformation($"{nameof(GetReadmeFunction)} Stared");

            var readmeFile = await _gitHubApiService.GetReadme(repository.Owner, repository.Name).ConfigureAwait(false);
            var readmeText = await _httpClient.GetStringAsync(readmeFile.DownloadUrl).ConfigureAwait(false);

            log.LogInformation($"{nameof(GetReadmeFunction)} Completed");

            return new Repository(repository.Owner, repository.Name, readmeText);
        }
    }
}
