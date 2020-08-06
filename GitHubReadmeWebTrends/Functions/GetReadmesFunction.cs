using System;
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
        public async Task<(Repository, CloudAdvocateGitHubUserModel)> Run([QueueTrigger(QueueConstants.RepositoriesQueue)] (Repository, CloudAdvocateGitHubUserModel) data, ILogger log)
        {
            log.LogInformation($"{nameof(GetReadmeFunction)} Stared");

            var (repository, gitHubUser) = data;

            try
            {
                var readmeFile = await _gitHubApiService.GetReadme(repository.Owner, repository.Name).ConfigureAwait(false);
                var readmeText = await _httpClient.GetStringAsync(readmeFile.DownloadUrl).ConfigureAwait(false);

                log.LogInformation($"{nameof(GetReadmeFunction)} Completed");

                return (new Repository(repository.Owner, repository.Name, readmeText), gitHubUser);
            }
            catch (Exception e)
            {
                log.LogError(e, e.Message);
                throw;
            }
        }
    }
}
