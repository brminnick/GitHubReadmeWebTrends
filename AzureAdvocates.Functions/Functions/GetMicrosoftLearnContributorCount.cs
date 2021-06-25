using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GitHubApiStatus;
using GitHubReadmeWebTrends.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace AzureAdvocates.Functions
{
    class GetMicrosoftLearnContributorCount
    {
        readonly BlobStorageService _blobStorageService;
        readonly AdvocateService _advocateService;
        readonly IGitHubApiStatusService _gitHubApiStatusService;
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;

        public GetMicrosoftLearnContributorCount(AdvocateService advocateService,
                                                    BlobStorageService blobStorageService,
                                                    IGitHubApiStatusService gitHubApiStatusService,
                                                    GitHubGraphQLApiService gitHubGraphQLApiService)
        {
            _advocateService = advocateService;
            _blobStorageService = blobStorageService;
            _gitHubApiStatusService = gitHubApiStatusService;
            _gitHubGraphQLApiService = gitHubGraphQLApiService;
        }

        [Function(nameof(GetMicrosoftLearnContributorCount))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = nameof(GetMicrosoftLearnContributorCount) + "/{from:datetime}/{to:datetime}/{team?}")] HttpRequestData req, DateTime from, DateTime to, string? team, FunctionContext context)
        {
            var log = context.GetLogger<GetMicrosoftLearnContributorCount>();
            log.LogInformation($"{nameof(GetMicrosoftLearnContributorCount)} Started");

            var microsoftLearnContributionsList = await _blobStorageService.GetCloudAdvocateMicrosoftLearnContributors().ConfigureAwait(false) ?? Array.Empty<CloudAdvocateGitHubContributorModel>();

            int advocateCount = 0, advocateContributorCount = 0;
            var teamContributionCount = new SortedDictionary<string, int>();
            foreach (var advocateContribution in microsoftLearnContributionsList)
            {
                if (team is null || advocateContribution.Team.Equals(team, StringComparison.OrdinalIgnoreCase))
                {
                    log.LogInformation($"Adding Advocate: {advocateContribution.Name}");
                    advocateCount++;

                    var filteredPullRequests = advocateContribution.PullRequests.Where(x => x.CreatedAt.IsWithinRange(from, to)).ToList();
                    if (filteredPullRequests.Any())
                    {
                        log.LogInformation($"Team: {advocateContribution.Team}");

                        if (teamContributionCount.ContainsKey(advocateContribution.Team))
                            teamContributionCount[advocateContribution.Team]++;
                        else
                            teamContributionCount.Add(advocateContribution.Team, 1);

                        log.LogInformation($"Total Contributions: {filteredPullRequests.Count}");
                        advocateContributorCount++;
                    }
                }
            }

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new AdovocatesTotalContributionsModel(advocateCount, advocateContributorCount, teamContributionCount)).ConfigureAwait(false);

            return response;
        }
    }
}
