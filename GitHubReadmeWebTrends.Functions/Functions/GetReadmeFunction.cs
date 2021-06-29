using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GitHubApiStatus;
using GitHubReadmeWebTrends.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GitHubReadmeWebTrends.Functions
{
    public class GetReadmeFunction
    {
        const int _minimumApiRequests = 2000;
        const string _runEveryHour = "0 0 * * * *";

        readonly HttpClient _httpClient;
        readonly CloudQueueClient _cloudQueueClient;
        readonly GitHubRestApiService _gitHubRestApiService;
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;
        readonly IGitHubApiStatusService _gitHubApiStatusService;

        public GetReadmeFunction(CloudQueueClient cloudQueueClient,
                                    IHttpClientFactory httpClientFactory,
                                    GitHubRestApiService gitHubApiService,
                                    IGitHubApiStatusService gitHubApiStatusService,
                                    GitHubGraphQLApiService gitHubGraphQLApiService)
        {
            _httpClient = httpClientFactory.CreateClient();

            _cloudQueueClient = cloudQueueClient;
            _gitHubRestApiService = gitHubApiService;
            _gitHubApiStatusService = gitHubApiStatusService;
            _gitHubGraphQLApiService = gitHubGraphQLApiService;
        }

        [Function(nameof(GetReadmeQueueTriggerFunction))]
        public async Task<GetReadmeFunctionQueueOutputModel> GetReadmeQueueTriggerFunction([QueueTrigger(QueueConstants.RepositoriesQueue)] (Repository, AdvocateModel) data, FunctionContext context)
        {
            var log = context.GetLogger<GetReadmeFunction>();
            log.LogInformation($"{nameof(GetReadmeQueueTriggerFunction)} Stared");

            var remainingRepositoriesData = new List<(Repository, AdvocateModel)>();
            var completedRepositoriesData = new List<(Repository, AdvocateModel)>();

            var (repository, gitHubUser) = data;

            var getHubApiRateLimits = await _gitHubApiStatusService.GetApiRateLimits(CancellationToken.None).ConfigureAwait(false);

            // The GitHub API Limits requests to 5,0000 per hour https://docs.github.com/en/free-pro-team@latest/rest#rate-limiting
            // If the API Limit is approaching, output to RemainingRepositoriesQueue, where it will be handled by GetReadmeTimerTriggerFunction which runs once an hour
            // Otherwise, process the data and place it on VerifyWebTrendsQueue
            if (getHubApiRateLimits.RestApi.RemainingRequestCount < _minimumApiRequests
                || getHubApiRateLimits.GraphQLApi.RemainingRequestCount < _minimumApiRequests)
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

            return new GetReadmeFunctionQueueOutputModel
            {
                CompletedRepositoriesData = completedRepositoriesData,
                RemainingRepositoriesData = remainingRepositoriesData
            };
        }


        [Function(nameof(GetReadmeTimerTriggerFunction))]
        public async Task<GetReadmeFunctionQueueOutputModel> GetReadmeTimerTriggerFunction([TimerTrigger(_runEveryHour)] TimerInfo myTimer, FunctionContext context)
        {
            const int getMessageCount = 32;

            var log = context.GetLogger<GetReadmeFunction>();

            var remainingRepositoriesData = new List<(Repository, AdvocateModel)>();
            var completedRepositoriesData = new List<(Repository, AdvocateModel)>();

            var remainingRepositoriesQueue = _cloudQueueClient.GetQueueReference(QueueConstants.RemainingRepositoriesQueue.ToLower());

            log.LogInformation($"{nameof(GetReadmeFunction)} Stared");

            var queueResponse = await remainingRepositoriesQueue.GetMessagesAsync(getMessageCount).ConfigureAwait(false);

            while (queueResponse.Any())
            {
                foreach (var queueMessage in queueResponse)
                {
                    log.LogInformation($"Queue Message Id: {queueMessage.Id}");

                    var dequeuedData = JsonConvert.DeserializeObject<(Repository, AdvocateModel)>(queueMessage.AsString);
                    var (repository, gitHubUser) = dequeuedData;

                    var getHubApiRateLimits = await _gitHubApiStatusService.GetApiRateLimits(CancellationToken.None).ConfigureAwait(false);

                    if (getHubApiRateLimits.RestApi.RemainingRequestCount < _minimumApiRequests
                        || getHubApiRateLimits.GraphQLApi.RemainingRequestCount < _minimumApiRequests)
                    {
                        log.LogInformation($"Maximum API Requests Reached");

                        remainingRepositoriesData.Add(dequeuedData);

                        log.LogInformation($"Re-added Data to RemainingRepositoriesQueue");
                    }
                    else
                    {
                        try
                        {
                            await RetrieveReadme(repository, gitHubUser, log, completedRepositoriesData).ConfigureAwait(false);
                            await remainingRepositoriesQueue.DeleteMessageAsync(queueMessage).ConfigureAwait(false);
                        }
                        catch (Refit.ApiException e) when (e.StatusCode is System.Net.HttpStatusCode.NotFound)
                        {
                            await remainingRepositoriesQueue.DeleteMessageAsync(queueMessage).ConfigureAwait(false);
                        }
                    }
                }

                queueResponse = await remainingRepositoriesQueue.GetMessagesAsync(getMessageCount).ConfigureAwait(false);
            }

            log.LogInformation($"{nameof(GetReadmeFunction)} Completed");

            return new GetReadmeFunctionQueueOutputModel
            {
                CompletedRepositoriesData = completedRepositoriesData,
                RemainingRepositoriesData = remainingRepositoriesData
            };
        }

        async Task RetrieveReadme(Repository repository, AdvocateModel advocateModel, ILogger log, IList<(Repository, AdvocateModel)> completedRepositoriesData)
        {
            var readmeFile = await _gitHubRestApiService.GetReadme(repository.Owner, repository.Name).ConfigureAwait(false);
            var readmeText = await _httpClient.GetStringAsync(readmeFile.DownloadUrl).ConfigureAwait(false);

            completedRepositoriesData.Add((new Repository(repository.Id, repository.Owner, repository.Name, repository.DefaultBranchOid, repository.DefaultBranchPrefix, repository.DefaultBranchName, repository.IsFork, readmeText), advocateModel));

            log.LogInformation($"Found Readme for {repository.Owner} {repository.Name}");
        }

        public class GetReadmeFunctionQueueOutputModel
        {
            [QueueOutput(QueueConstants.RemainingRepositoriesQueue)]
            public IReadOnlyList<(Repository, AdvocateModel)> RemainingRepositoriesData { get; init; } = Array.Empty<(Repository, AdvocateModel)>();

            [QueueOutput(QueueConstants.VerifyWebTrendsQueue)]
            public IReadOnlyList<(Repository, AdvocateModel)> CompletedRepositoriesData { get; init; } = Array.Empty<(Repository, AdvocateModel)>();
        }
    }
}
