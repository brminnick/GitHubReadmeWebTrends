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
    public class GetDocsContributorCount
    {
        readonly CloudAdvocateService _cloudAdvocateService;
        readonly IGitHubApiStatusService _gitHubApiStatusService;
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;

        public GetDocsContributorCount(CloudAdvocateService cloudAdvocateService,
                                        IGitHubApiStatusService gitHubApiStatusService,
                                        GitHubGraphQLApiService gitHubGraphQLApiService)
        {
            _cloudAdvocateService = cloudAdvocateService;
            _gitHubApiStatusService = gitHubApiStatusService;
            _gitHubGraphQLApiService = gitHubGraphQLApiService;
        }

        [FunctionName(nameof(GetDocsContributorCount))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = nameof(GetDocsContributorCount) + "/{from:datetime}/{to:datetime}/{team?}")] HttpRequestMessage req, DateTime from, DateTime to, string? team, ILogger log)
        {
            log.LogInformation($"{nameof(GetDocsContributorCount)} Started");

            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            var gitHubApiStatus = await _gitHubApiStatusService.GetApiRateLimits(cancellationTokenSource.Token).ConfigureAwait(false);
            if (gitHubApiStatus.GraphQLApi.RemainingRequestCount < 4000)
            {
                return new ObjectResult($"Maximum GitHub API Limit Reached. GitHub API Limit will reset in {gitHubApiStatus.GraphQLApi.RateLimitReset_TimeRemaining.Minutes + 1} minute(s). Try again at {gitHubApiStatus.GraphQLApi.RateLimitReset_DateTime.UtcDateTime} UTC")
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }

            int advocateCount = 0, advocateContributorCount = 0;
            await foreach (var advocate in _cloudAdvocateService.GetAzureAdvocates().ConfigureAwait(false))
            {
                if (team is not null && advocate.MicrosoftTeam != team)
                    continue;

                log.LogInformation($"Found {advocate.FullName}");
                advocateCount++;

                var microsoftDocsContributions = await _gitHubGraphQLApiService.GetMicrosoftDocsContributionsCollection(advocate.GitHubUserName, from, to).ConfigureAwait(false);

                if (microsoftDocsContributions.TotalContributions > 0)
                {
                    log.LogInformation($"Total Contributions: {microsoftDocsContributions.TotalContributions}");
                    advocateContributorCount++;
                }
            }

            return new OkObjectResult(new AdovocatesTotalContributionsModel(advocateContributorCount, advocateCount));
        }
    }
}
