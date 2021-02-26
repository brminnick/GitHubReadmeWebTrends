using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GitHubApiStatus;
using GitHubReadmeWebTrends.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace AzureAdvocates.Functions
{
    class GetDocsContributorCount
    {
        readonly BlobStorageService _blobStorageService;
        readonly CloudAdvocateService _cloudAdvocateService;
        readonly IGitHubApiStatusService _gitHubApiStatusService;
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;

        public GetDocsContributorCount(BlobStorageService blobStorageService,
                                        CloudAdvocateService cloudAdvocateService,
                                        IGitHubApiStatusService gitHubApiStatusService,
                                        GitHubGraphQLApiService gitHubGraphQLApiService)
        {
            _blobStorageService = blobStorageService;
            _cloudAdvocateService = cloudAdvocateService;
            _gitHubApiStatusService = gitHubApiStatusService;
            _gitHubGraphQLApiService = gitHubGraphQLApiService;
        }

        [FunctionName(nameof(GetDocsContributorCount))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = nameof(GetDocsContributorCount) + "/{from:datetime}/{to:datetime}/{team?}")] HttpRequestMessage req, DateTime from, DateTime to, string? team, ILogger log)
        {
            log.LogInformation($"{nameof(GetDocsContributorCount)} Started");

            var requestedTimeSpan = to - from;
            if (requestedTimeSpan.TotalDays > 366)
                return new BadRequestObjectResult("Date Range Must Be Less Than A Year");

            var gitHubGraphQLApiStatus = await GetGraphQLRateLimitStatus(_gitHubApiStatusService).ConfigureAwait(false);
            if (gitHubGraphQLApiStatus.RemainingRequestCount < 4000)
            {
                return new ObjectResult($"Maximum GitHub API Limit Reached. GitHub API Limit will reset in {gitHubGraphQLApiStatus.RateLimitReset_TimeRemaining.Minutes + 1} minute(s). Try again at {gitHubGraphQLApiStatus.RateLimitReset_DateTime.UtcDateTime} UTC")
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }

            var advocatesContributionModel = await GetTotalContributions(_cloudAdvocateService, _gitHubGraphQLApiService, log, from, to, team).ConfigureAwait(false);
            return new OkObjectResult(advocatesContributionModel);
        }

        static async Task<IRateLimitStatus> GetGraphQLRateLimitStatus(IGitHubApiStatusService gitHubApiStatusService)
        {
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            var gitHubApiStatus = await gitHubApiStatusService.GetApiRateLimits(cancellationTokenSource.Token).ConfigureAwait(false);

            return gitHubApiStatus.GraphQLApi;
        }

        static async Task<AdovocatesTotalContributionsModel> GetTotalContributions(CloudAdvocateService cloudAdvocateService, GitHubGraphQLApiService gitHubGraphQLApiService, ILogger log, DateTime from, DateTime to, string? requestedTeam)
        {
            int advocateCount = 0, advocateContributorCount = 0;
            var teamContributionCount = new SortedDictionary<string, int>();
            await foreach (var advocate in cloudAdvocateService.GetAzureAdvocates().ConfigureAwait(false))
            {
                if (requestedTeam is not null && advocate.MicrosoftTeam != requestedTeam)
                    continue;

                log.LogInformation($"Found {advocate.FullName}");
                advocateCount++;

                var microsoftDocsContributions = await gitHubGraphQLApiService.GetMicrosoftDocsContributionsCollection(advocate.GitHubUserName, from, to).ConfigureAwait(false);

                if (microsoftDocsContributions.TotalPullRequestContributions
                    + microsoftDocsContributions.TotalPullRequestReviewContributions
                    + microsoftDocsContributions.TotalCommitContributions > 0)
                {
                    log.LogInformation($"Team: {advocate.MicrosoftTeam}");

                    if (teamContributionCount.ContainsKey(advocate.MicrosoftTeam))
                        teamContributionCount[advocate.MicrosoftTeam]++;
                    else
                        teamContributionCount.Add(advocate.MicrosoftTeam, 1);

                    log.LogInformation($"Total Contributions: {microsoftDocsContributions.TotalContributions}");
                    advocateContributorCount++;
                }
            }

            return new AdovocatesTotalContributionsModel(advocateCount, advocateContributorCount, teamContributionCount);
        }
    }
}
