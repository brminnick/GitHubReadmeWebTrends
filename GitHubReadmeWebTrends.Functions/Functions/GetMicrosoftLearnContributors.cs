using System;
using System.Collections.Generic;
using System.Linq;
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

namespace GitHubReadmeWebTrends.Functions
{
    class GetMicrosoftLearnContributors
    {
        readonly CloudAdvocateService _cloudAdvocateService;
        readonly IGitHubApiStatusService _gitHubApiStatusService;
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;

        public GetMicrosoftLearnContributors(CloudAdvocateService cloudAdvocateService,
                                                IGitHubApiStatusService gitHubApiStatusService,
                                                GitHubGraphQLApiService_PrivateRepoAccess gitHubGraphQLApiService)
        {
            _cloudAdvocateService = cloudAdvocateService;
            _gitHubApiStatusService = gitHubApiStatusService;
            _gitHubGraphQLApiService = gitHubGraphQLApiService;
        }

        [FunctionName(nameof(GetMicrosoftLearnContributors))]
        public async Task<IActionResult> RunHttpTrigger([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = nameof(GetMicrosoftLearnContributors) + "/{from:datetime}/{to:datetime}/{team:string}")] HttpRequestMessage req, DateTime from, DateTime to, ILogger log, string team = "")
        {
            log.LogInformation($"{nameof(GetMicrosoftLearnContributors)} Started");

            var timeSpan = to - from;
            if (timeSpan.TotalDays > 365)
                return new BadRequestObjectResult($"Invalid Timespan: {timeSpan.TotalDays} days. Timespan must be less than 365 days");

            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            var gitHubApiStatus = await _gitHubApiStatusService.GetApiRateLimits(cancellationTokenSource.Token).ConfigureAwait(false);
            if (gitHubApiStatus.GraphQLApi.RemainingRequestCount < 2000)
            {
                return new ObjectResult($"Maximum GitHub API Limit Reached. GitHub API Limit will reset in {gitHubApiStatus.GraphQLApi.RateLimitReset_TimeRemaining}. Try again at {gitHubApiStatus.GraphQLApi.RateLimitReset_DateTime}")
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                };
            }

            var gitHubContributionsList = new List<GitHubContributorModel>();
            await foreach (var cloudAdvocateGitHubUserModel in _cloudAdvocateService.GetAzureAdvocates().ConfigureAwait(false))
            {
                if (cloudAdvocateGitHubUserModel.MicrosoftTeam.Equals(team, StringComparison.OrdinalIgnoreCase))
                {
                    var contributions = await _gitHubGraphQLApiService.GetMicrosoftDocsContributionsCollection(cloudAdvocateGitHubUserModel.GitHubUserName, from, to).ConfigureAwait(false);
                    gitHubContributionsList.Add(new GitHubContributorModel(contributions, cloudAdvocateGitHubUserModel));
                }
            }

            return new OkObjectResult(gitHubContributionsList);
        }
    }
}
