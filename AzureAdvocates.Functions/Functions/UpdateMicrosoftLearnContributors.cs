using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitHubApiStatus;
using GitHubReadmeWebTrends.Common;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AzureAdvocates.Functions
{
    class UpdateMicrosoftLearnContributors
    {
        readonly BlobStorageService _blobStorageService;
        readonly AdvocateService _advocateService;
        readonly IGitHubApiStatusService _gitHubApiStatusService;
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;

        public UpdateMicrosoftLearnContributors(AdvocateService advocateService,
                                                BlobStorageService blobStorageService,
                                                IGitHubApiStatusService gitHubApiStatusService,
                                                GitHubGraphQLApiService gitHubGraphQLApiService)
        {
            _advocateService = advocateService;
            _blobStorageService = blobStorageService;
            _gitHubApiStatusService = gitHubApiStatusService;
            _gitHubGraphQLApiService = gitHubGraphQLApiService;
        }

        [FunctionName(nameof(UpdateMicrosoftLearnContributors))]
        public async Task Run([TimerTrigger("0 0 */6 * * *")] TimerInfo timer, ILogger log)
        {
            log.LogInformation($"{nameof(UpdateMicrosoftLearnContributors)} Started");

            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var gitHubApiStatus = await _gitHubApiStatusService.GetApiRateLimits(cancellationTokenSource.Token).ConfigureAwait(false);
            if (gitHubApiStatus.GraphQLApi.RemainingRequestCount < 4000)
            {
                log.LogError($"Maximum GitHub API Limit Reached. GitHub API Limit will reset in {gitHubApiStatus.GraphQLApi.RateLimitReset_TimeRemaining.Minutes + 1} minute(s). Try again at {gitHubApiStatus.GraphQLApi.RateLimitReset_DateTime.UtcDateTime} UTC");
                return;
            }

            var advocateList = await _advocateService.GetCurrentAdvocates(cancellationTokenSource.Token).ConfigureAwait(false);

            var microsoftLearnPullRequests = new List<RepositoryPullRequest>();
            await foreach (var pullRequestList in _gitHubGraphQLApiService.GetMicrosoftLearnPullRequests().ConfigureAwait(false))
            {
                microsoftLearnPullRequests.AddRange(pullRequestList);
                log.LogInformation($"Added {pullRequestList.Count} Pull Requests from {pullRequestList.FirstOrDefault()?.RepositoryName}");
            }

            var cloudAdvocateContributions = new List<CloudAdvocateGitHubContributorModel>();
            foreach (var cloudAdvocate in advocateList)
            {
                var cloudAdvocateContributorModel = new CloudAdvocateGitHubContributorModel(microsoftLearnPullRequests.Where(x => x.Author.Equals(cloudAdvocate.GitHubUsername, StringComparison.OrdinalIgnoreCase)), cloudAdvocate);

                cloudAdvocateContributions.Add(cloudAdvocateContributorModel);

                log.LogInformation($"Added {cloudAdvocateContributorModel.PullRequests.Count} Pull Requests for {cloudAdvocate.Name}");
            }

            var blobName = $"Contributions_{DateTime.UtcNow:o}.json";
            await _blobStorageService.UploadCloudAdvocateMicrosoftLearnContributions(cloudAdvocateContributions, blobName).ConfigureAwait(false);
        }
    }
}
