using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace VerifyGitHubReadmeLinks.Functions
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
        public async Task Run([QueueTrigger(QueueConstants.RepositoriesQueue)] (Repository, CloudAdvocateGitHubUserModel) data, ILogger log,
                                [Queue(QueueConstants.VerifyWebTrendsQueue)] ICollector<(Repository, CloudAdvocateGitHubUserModel)> outputData)
        {
            log.LogInformation($"{nameof(GetReadmeFunction)} Stared");

            var (repository, gitHubUser) = data;

            try
            {
                var readmeFile = await _gitHubApiService.GetReadme(repository.Owner, repository.Name).ConfigureAwait(false);
                var readmeText = await _httpClient.GetStringAsync(readmeFile.DownloadUrl).ConfigureAwait(false);

                outputData.Add((new Repository(repository.Id, repository.Owner, repository.Name, repository.DefaultBranchOid, repository.DefaultBranchPrefix, repository.DefaultBranchName, repository.IsFork, readmeText), gitHubUser));

                log.LogInformation($"Found Readme for {repository.Owner} {repository.Name}");
            }
            catch (Refit.ApiException e) when (e.StatusCode is System.Net.HttpStatusCode.NotFound)
            {
                //If a Readme doesn't exist, GitHubApiService.GetReadme will return a 404 Not Found response
            }

            log.LogInformation($"{nameof(GetReadmeFunction)} Completed");
        }
    }
}
