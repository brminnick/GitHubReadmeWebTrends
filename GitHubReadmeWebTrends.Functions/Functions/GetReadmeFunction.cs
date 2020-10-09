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
        readonly GitHubRestApiService _gitHubRestApiService;
        readonly CloudQueueClient _cloudQueueClient;

        public GetReadmeFunction(GitHubRestApiService gitHubApiService, IHttpClientFactory httpClientFactory, CloudQueueClient cloudQueueClient)
        {
            _httpClient = httpClientFactory.CreateClient();
            _gitHubRestApiService = gitHubApiService;
            _cloudQueueClient = cloudQueueClient;
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

                // The GitHub API Limits requests to 5,0000 per hour https://developer.github.com/v3/#rate-limiting
                // If the API Limit is approaching, output to RemainingRepositoriesQueue, where it will be handled by GetReadmeTimerTriggerFunction which runs once an hour
                // Otherwise, process the data and place it on VerifyWebTrendsQueue
                if (GitHubApiService.GetNumberOfApiRequestsRemaining(response.Headers) < 2000)
                {
                    log.LogInformation($"Maximum API Requests Reached");

                    remainingRepositoriesData.Add(data);

                    log.LogInformation($"Added Data to RemainingRepositoriesQueue");
                }
                else
                {
                    await RetrieveReadme(repository, gitHubUser, log, completedRepositoriesData).ConfigureAwait(false);
                }
            }
            catch (Refit.ApiException e) when (e.StatusCode is System.Net.HttpStatusCode.NotFound)
            {
                //If a Readme doesn't exist, GitHubApiService.GetReadme will return a 404 Not Found response
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
                        var response = await _gitHubRestApiService.GetResponseMessage().ConfigureAwait(false);

                        if (GitHubApiService.GetNumberOfApiRequestsRemaining(response.Headers) >= 2000)
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
