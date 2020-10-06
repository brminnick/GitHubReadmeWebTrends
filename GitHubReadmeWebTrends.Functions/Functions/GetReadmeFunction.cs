using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using GitHubReadmeWebTrends.Common;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GitHubReadmeWebTrends.Functions
{
    class GetReadmeFunction
    {
        const string _runEveryHour = "0 0 * * * *";

        readonly HttpClient _httpClient;
        readonly GitHubRestApiService _gitHubRestApiService;
        readonly RemainingRepositoriesQueueClient _remainingRepositoriesQueueClient;

        public GetReadmeFunction(GitHubRestApiService gitHubApiService, IHttpClientFactory httpClientFactory, RemainingRepositoriesQueueClient remainingRepositoriesQueueClient)
        {
            _httpClient = httpClientFactory.CreateClient();
            _gitHubRestApiService = gitHubApiService;
            _remainingRepositoriesQueueClient = remainingRepositoriesQueueClient;
        }
        [FunctionName(nameof(GetReadmeQueueTriggerFunction))]
        public async Task GetReadmeQueueTriggerFunction([QueueTrigger(QueueConstants.RepositoriesQueue)] (Repository, CloudAdvocateGitHubUserModel) data, ILogger log,
                                [Queue(QueueConstants.RemainingRepositoriesQueue)] ICollector<(Repository, CloudAdvocateGitHubUserModel)> remainingRepositoriesData,
                                [Queue(QueueConstants.VerifyWebTrendsQueue)] ICollector<(Repository, CloudAdvocateGitHubUserModel)> completedRepositoriesData)
        {
            log.LogInformation($"{nameof(GetReadmeFunction)} Stared");

            var (repository, gitHubUser) = data;

            try
            {
                var response = await _gitHubRestApiService.GetResponseMessage().ConfigureAwait(false);

                if (GitHubApiService.GetNumberOfApiRequestsRemaining(response.Headers) < 2000)
                {
                    log.LogInformation($"Maximum API Requests Reached");

                    remainingRepositoriesData.Add(data);

                    log.LogInformation($"Added Data to RemainingRepositoriesQueue");
                }
                else
                {
                    await RetrieveReadme(repository, log, completedRepositoriesData).ConfigureAwait(false);
                }
            }
            catch (Refit.ApiException e) when (e.StatusCode is System.Net.HttpStatusCode.NotFound)
            {
                //If a Readme doesn't exist, GitHubApiService.GetReadme will return a 404 Not Found response
            }

            log.LogInformation($"{nameof(GetReadmeFunction)} Completed");
        }


        [FunctionName(nameof(GetReadmeTimerTriggerFunction))]
        public async Task GetReadmeTimerTriggerFunction([TimerTrigger(_runEveryHour, RunOnStartup = false)] TimerInfo myTimer, ILogger log,
                                [Queue(QueueConstants.RemainingRepositoriesQueue)] ICollector<(Repository, CloudAdvocateGitHubUserModel)> remainingRepositoriesData,
                                [Queue(QueueConstants.VerifyWebTrendsQueue)] ICollector<(Repository, CloudAdvocateGitHubUserModel)> completedRepositoriesData)
        {
            log.LogInformation($"{nameof(GetReadmeFunction)} Stared");

            var queueResponse = await _remainingRepositoriesQueueClient.ReceiveMessagesAsync().ConfigureAwait(false);

            foreach (var queueMessage in queueResponse.Value)
            {
                var dequeuedData = JsonConvert.DeserializeObject<(Repository, CloudAdvocateGitHubUserModel)>(queueMessage.MessageText);
                var (repository, gitHubUser) = dequeuedData;

                try
                {
                    var response = await _gitHubRestApiService.GetResponseMessage().ConfigureAwait(false);

                    if (GitHubApiService.GetNumberOfApiRequestsRemaining(response.Headers) >= 2000)
                    {
                        await RetrieveReadme(repository, gitHubUser, log, completedRepositoriesData).ConfigureAwait(false);
                        await _remainingRepositoriesQueueClient.DeleteMessageAsync(queueMessage.MessageId, queueMessage.PopReceipt).ConfigureAwait(false);
                    }
                    else
                    {
                        remainingRepositoriesData.Add(dequeuedData);
                    }
                }
                catch (Refit.ApiException e) when (e.StatusCode is System.Net.HttpStatusCode.NotFound)
                {
                    //If a Readme doesn't exist, GitHubApiService.GetReadme will return a 404 Not Found response
                }

                log.LogInformation($"{nameof(GetReadmeFunction)} Completed");
            }
        }

        async Task RetrieveReadme(Repository repository, CloudAdvocateGitHubUserModel gitHubUser, ILogger log, ICollector<(Repository, CloudAdvocateGitHubUserModel)> completedRepositoriesData)
        {
            var readmeFile = await _gitHubRestApiService.GetReadme(repository.Owner, repository.Name).ConfigureAwait(false);
            var readmeText = await _httpClient.GetStringAsync(readmeFile.DownloadUrl).ConfigureAwait(false);

            completedRepositoriesData.Add((new Repository(repository.Id, repository.Owner, repository.Name, repository.DefaultBranchOid, repository.DefaultBranchPrefix, repository.DefaultBranchName, repository.IsFork, readmeText), gitHubUser));

            log.LogInformation($"Found Readme for {repository.Owner} {repository.Name}");
        }
    }
}
