using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GitHubReadmeWebTrends.Common;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GitHubReadmeWebTrends.Functions
{
    public class GetReadmeFunction
    {
        const string _runEveryHour = "0 0 * * * *";

        readonly HttpClient _httpClient;
        readonly CloudQueueClient _cloudQueueClient;
        readonly GitHubRestApiService _gitHubRestApiService;
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;
        readonly GitHubApiStatusService _gitHubApiStatusService;

        public GetReadmeFunction(CloudQueueClient cloudQueueClient,
                                    IHttpClientFactory httpClientFactory,
                                    GitHubRestApiService gitHubApiService,
                                    GitHubApiStatusService gitHubApiStatusService,
                                    GitHubGraphQLApiService gitHubGraphQLApiService)
        {
            _httpClient = httpClientFactory.CreateClient();

            _cloudQueueClient = cloudQueueClient;
            _gitHubRestApiService = gitHubApiService;
            _gitHubApiStatusService = gitHubApiStatusService;
            _gitHubGraphQLApiService = gitHubGraphQLApiService;
        }

        [FunctionName(nameof(GetReadmeQueueTriggerFunction))]
        public async Task GetReadmeQueueTriggerFunction([QueueTrigger(QueueConstants.RepositoriesQueue)] (Repository, CloudAdvocateGitHubUserModel) data, ILogger log,
                                [Queue(QueueConstants.RemainingRepositoriesQueue)] ICollector<(Repository, CloudAdvocateGitHubUserModel)> remainingRepositoriesData,
                                [Queue(QueueConstants.VerifyWebTrendsQueue)] ICollector<(Repository, CloudAdvocateGitHubUserModel)> completedRepositoriesData)
        {
            const int minimumApiRequests = 2000;

            log.LogInformation($"{nameof(GetReadmeFunction)} Stared");

            var (repository, gitHubUser) = data;

            var (gitHubRestApiRequestsRemaining, gitHubGraphQLApiRequestsRemaining) = await _gitHubApiStatusService.GetRemaininRequestCount().ConfigureAwait(false);

            log.LogInformation($"{nameof(gitHubRestApiRequestsRemaining)}: {gitHubRestApiRequestsRemaining}");
            log.LogInformation($"{nameof(gitHubGraphQLApiRequestsRemaining)}: {gitHubGraphQLApiRequestsRemaining}");


            // The GitHub API Limits requests to 5,0000 per hour https://docs.github.com/en/free-pro-team@latest/rest#rate-limiting
            // If the API Limit is approaching, output to RemainingRepositoriesQueue, where it will be handled by GetReadmeTimerTriggerFunction which runs once an hour
            // Otherwise, process the data and place it on VerifyWebTrendsQueue
            if (gitHubRestApiRequestsRemaining < minimumApiRequests
                || gitHubGraphQLApiRequestsRemaining < minimumApiRequests)
            {
                log.LogInformation($"Maximum API Requests Reached");

                remainingRepositoriesData.Add(data);

                log.LogInformation($"Added Data to RemainingRepositoriesQueue");
            }
            else
            {
                try
                {
                    await RetrieveReadme(repository, gitHubUser, log, completedRepositoriesData).ConfigureAwait(false);
                }
                catch (Refit.ApiException e) when (e.StatusCode is System.Net.HttpStatusCode.NotFound)
                {
                    //If a Readme doesn't exist, GitHubApiService.GetReadme will return a 404 Not Found response
                }
            }

            log.LogInformation($"{nameof(GetReadmeFunction)} Completed");
        }


        [FunctionName(nameof(GetReadmeTimerTriggerFunction))]
        public async Task GetReadmeTimerTriggerFunction([TimerTrigger(_runEveryHour, RunOnStartup = true)] TimerInfo myTimer, ILogger log,
                                [Queue(QueueConstants.RemainingRepositoriesQueue)] ICollector<(Repository, CloudAdvocateGitHubUserModel)> remainingRepositoriesData,
                                [Queue(QueueConstants.VerifyWebTrendsQueue)] ICollector<(Repository, CloudAdvocateGitHubUserModel)> completedRepositoriesData)
        {
            const int getMessageCount = 32;

            var remainingRepositoriesQueue = _cloudQueueClient.GetQueueReference(QueueConstants.RemainingRepositoriesQueue.ToLower());

            log.LogInformation($"{nameof(GetReadmeFunction)} Stared");

            var queueResponse = await remainingRepositoriesQueue.GetMessagesAsync(getMessageCount).ConfigureAwait(false);

            while (queueResponse.Any())
            {
                foreach (var queueMessage in queueResponse)
                {
                    log.LogInformation($"Queue Message Id: {queueMessage.Id}");

                    var dequeuedData = JsonConvert.DeserializeObject<(Repository, CloudAdvocateGitHubUserModel)>(queueMessage.AsString);
                    var (repository, gitHubUser) = dequeuedData;

                    try
                    {
                        var (gitHubRestApiRequestsRemaining, gitHubGraphQLApiRequestsRemaining) = await _gitHubApiStatusService.GetRemaininRequestCount().ConfigureAwait(false);

                        if (gitHubRestApiRequestsRemaining > 2000
                            && gitHubGraphQLApiRequestsRemaining > 2000)
                        {
                            await RetrieveReadme(repository, gitHubUser, log, completedRepositoriesData).ConfigureAwait(false);
                            await remainingRepositoriesQueue.DeleteMessageAsync(queueMessage).ConfigureAwait(false);
                        }
                        else
                        {
                            remainingRepositoriesData.Add(dequeuedData);
                        }
                    }
                    catch (Refit.ApiException e) when (e.StatusCode is System.Net.HttpStatusCode.NotFound)
                    {
                        await remainingRepositoriesQueue.DeleteMessageAsync(queueMessage).ConfigureAwait(false);
                    }
                }

                queueResponse = await remainingRepositoriesQueue.GetMessagesAsync(getMessageCount).ConfigureAwait(false);
            }

            log.LogInformation($"{nameof(GetReadmeFunction)} Completed");
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
