using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
        const string _defaultTeam = "";

        readonly CloudAdvocateService _cloudAdvocateService;
        readonly IGitHubApiStatusService _gitHubApiStatusService;
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;

        public GetMicrosoftLearnContributors(CloudAdvocateService cloudAdvocateService,
                                                IGitHubApiStatusService gitHubApiStatusService,
                                                GitHubGraphQLApiService gitHubGraphQLApiService)
        {
            _cloudAdvocateService = cloudAdvocateService;
            _gitHubApiStatusService = gitHubApiStatusService;
            _gitHubGraphQLApiService = gitHubGraphQLApiService;
        }

        [FunctionName(nameof(GetMicrosoftLearnContributors))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = nameof(GetMicrosoftLearnContributors) + "/{from:datetime}/{to:datetime}/{team?}")] HttpRequestMessage req, DateTime from, DateTime to, string? team, ILogger log)
        {
            log.LogInformation($"{nameof(GetMicrosoftLearnContributors)} Started");

            var timeSpan = to - from;
            if (timeSpan.TotalDays > 365)
                return new BadRequestObjectResult($"Invalid Timespan: {timeSpan.TotalDays} days. Timespan must be less than 365 days");

            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            var gitHubApiStatus = await _gitHubApiStatusService.GetApiRateLimits(cancellationTokenSource.Token).ConfigureAwait(false);
            if (gitHubApiStatus.GraphQLApi.RemainingRequestCount < 4000)
            {
                return new ObjectResult($"Maximum GitHub API Limit Reached. GitHub API Limit will reset in {gitHubApiStatus.GraphQLApi.RateLimitReset_TimeRemaining.Minutes + 1} minute(s). Try again at {gitHubApiStatus.GraphQLApi.RateLimitReset_DateTime.UtcDateTime} UTC")
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                };
            }

            var cloudAdvocateList = new List<CloudAdvocateGitHubUserModel>();
            await foreach (var advocate in _cloudAdvocateService.GetAzureAdvocates().ConfigureAwait(false))
            {
                if (team is null || advocate.MicrosoftTeam.Equals(team, StringComparison.OrdinalIgnoreCase))
                {
                    log.LogInformation($"Found Advocate: {advocate.FullName}");
                    cloudAdvocateList.Add(advocate);
                }
            }

            var microsoftLearnPullRequests = new List<RepositoryPullRequest>();
            await foreach (var pullRequestList in _gitHubGraphQLApiService.GetMicrosoftLearnPullRequests().ConfigureAwait(false))
            {
                microsoftLearnPullRequests.AddRange(pullRequestList);
                log.LogInformation($"Added {pullRequestList.Count} Pull Requests from {pullRequestList.FirstOrDefault()?.RepositoryName}");
            }

            var cloudAdvocateContributions = new List<GitHubContributorModel>();
            foreach (var cloudAdvocate in cloudAdvocateList)
            {
                var cloudAdvocateContributorModel = new GitHubContributorModel(microsoftLearnPullRequests.Where(x => x.Author.Equals(cloudAdvocate.GitHubUserName, StringComparison.OrdinalIgnoreCase)), cloudAdvocate);

                cloudAdvocateContributions.Add(cloudAdvocateContributorModel);

                log.LogInformation($"Added {cloudAdvocateContributorModel.PullRequests.Count} Pull Requests for {cloudAdvocate.FullName}");
            }

            return new OkObjectResult(cloudAdvocateContributions);
        }
    }
}
