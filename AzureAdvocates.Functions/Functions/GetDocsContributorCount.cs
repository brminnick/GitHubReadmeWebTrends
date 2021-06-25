using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GitHubApiStatus;
using GitHubReadmeWebTrends.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace AzureAdvocates.Functions
{
    class GetDocsContributorCount
    {
        readonly AdvocateService _advocateService;
        readonly BlobStorageService _blobStorageService;
        readonly IGitHubApiStatusService _gitHubApiStatusService;
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;

        public GetDocsContributorCount(AdvocateService advocateService,
                                        BlobStorageService blobStorageService,
                                        IGitHubApiStatusService gitHubApiStatusService,
                                        GitHubGraphQLApiService gitHubGraphQLApiService)
        {
            _advocateService = advocateService;
            _blobStorageService = blobStorageService;
            _gitHubApiStatusService = gitHubApiStatusService;
            _gitHubGraphQLApiService = gitHubGraphQLApiService;
        }

        [Function(nameof(GetDocsContributorCount))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = nameof(GetDocsContributorCount) + "/{fromDateTime}/{toDateTime}/{team?}")] HttpRequestData req, string fromDateTime, string toDateTime, string? team, FunctionContext context)
        {
            var log = context.GetLogger<GetDocsContributorCount>();
            log.LogInformation($"{nameof(GetDocsContributorCount)} Started");

            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            var isFromValid = DateTime.TryParse(fromDateTime, out var from);
            var isToValid = DateTime.TryParse(toDateTime, out var to);

            if(!isFromValid || !isToValid)
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteStringAsync("Invalid Dates Provided").ConfigureAwait(false);

                return response;
            }

            var requestedTimeSpan = to - from;
            if (requestedTimeSpan.TotalDays > 366)
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteStringAsync("Date Range Must Be Less Than A Year").ConfigureAwait(false);

                return response;
            }

            var gitHubGraphQLApiStatus = await GetGraphQLRateLimitStatus(_gitHubApiStatusService).ConfigureAwait(false);
            if (gitHubGraphQLApiStatus.RemainingRequestCount < 4000)
            {
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync($"Maximum GitHub API Limit Reached. GitHub API Limit will reset in {gitHubGraphQLApiStatus.RateLimitReset_TimeRemaining.Minutes + 1} minute(s). Try again at {gitHubGraphQLApiStatus.RateLimitReset_DateTime.UtcDateTime} UTC").ConfigureAwait(false);

                return response;
            }

            var advocatesContributionModel = await GetTotalContributions(_advocateService, _gitHubGraphQLApiService, log, from, to, team, cancellationTokenSource.Token).ConfigureAwait(false);

            var okResponse = req.CreateResponse(HttpStatusCode.OK);
            await okResponse.WriteAsJsonAsync(advocatesContributionModel).ConfigureAwait(false);

            return okResponse;
        }

        static async Task<IRateLimitStatus> GetGraphQLRateLimitStatus(IGitHubApiStatusService gitHubApiStatusService)
        {
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            var gitHubApiStatus = await gitHubApiStatusService.GetApiRateLimits(cancellationTokenSource.Token).ConfigureAwait(false);

            return gitHubApiStatus.GraphQLApi;
        }

        static async Task<AdovocatesTotalContributionsModel> GetTotalContributions(AdvocateService advocateService, GitHubGraphQLApiService gitHubGraphQLApiService, ILogger log, DateTime from, DateTime to, string? requestedTeam, CancellationToken cancellationToken)
        {
            int advocateCount = 0, advocateContributorCount = 0;
            var teamContributionCount = new SortedDictionary<string, int>();

            var currentAdvocates = await advocateService.GetCurrentAdvocates(cancellationToken).ConfigureAwait(false);

            foreach (var advocate in currentAdvocates)
            {
                if (requestedTeam is not null && advocate.Team != requestedTeam)
                    continue;

                log.LogInformation($"Found {advocate.Name}");
                advocateCount++;

                var microsoftDocsContributions = await gitHubGraphQLApiService.GetMicrosoftDocsContributionsCollection(advocate.GitHubUsername, from, to).ConfigureAwait(false);

                log.LogInformation($"Team: {advocate.Team}");
                if (!teamContributionCount.ContainsKey(advocate.Team))
                    teamContributionCount.Add(advocate.Team, 0);

                if (microsoftDocsContributions.TotalPullRequestContributions
                    + microsoftDocsContributions.TotalPullRequestReviewContributions
                    + microsoftDocsContributions.TotalCommitContributions > 0)
                {
                    log.LogInformation($"Total Contributions: {microsoftDocsContributions.TotalContributions}");
                    advocateContributorCount++;
                    teamContributionCount[advocate.Team]++;
                }
            }

            return new AdovocatesTotalContributionsModel(advocateCount, advocateContributorCount, teamContributionCount);
        }
    }
}
