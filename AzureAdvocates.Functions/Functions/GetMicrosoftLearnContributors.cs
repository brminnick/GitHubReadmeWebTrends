using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace AzureAdvocates.Functions
{
    class GetMicrosoftLearnContributors
    {
        readonly BlobStorageService _blobStorageService;

        public GetMicrosoftLearnContributors(BlobStorageService blobStorageService) => _blobStorageService = blobStorageService;

        [Function(nameof(GetMicrosoftLearnContributors))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = nameof(GetMicrosoftLearnContributors) + "/{from:datetime}/{to:datetime}/{team?}")] HttpRequestData req, DateTime from, DateTime to, string? team, FunctionContext context)
        {
            var log = context.GetLogger<GetMicrosoftLearnContributors>();
            log.LogInformation($"{nameof(GetMicrosoftLearnContributors)} Started");

            var microsoftLearnContributionsList = await _blobStorageService.GetCloudAdvocateMicrosoftLearnContributors().ConfigureAwait(false);

            var filteredCloudAdvocateContributions = new List<CloudAdvocateGitHubContributorModel>();
            foreach (var advocateContribution in microsoftLearnContributionsList)
            {
                if (team is null || advocateContribution.Team.Equals(team, StringComparison.OrdinalIgnoreCase))
                {
                    log.LogInformation($"Adding Advocate: {advocateContribution.Name}");

                    var filteredPullRequests = advocateContribution.PullRequests.Where(x => x.CreatedAt.IsWithinRange(from, to)).ToList();
                    var filteredCloudAdvocateContribution = new CloudAdvocateGitHubContributorModel(filteredPullRequests, advocateContribution.Name, advocateContribution.GitHubUsername, advocateContribution.MicrosoftAlias, advocateContribution.Team, advocateContribution.RedditUserName);

                    filteredCloudAdvocateContributions.Add(filteredCloudAdvocateContribution);
                }
            }

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteAsJsonAsync(filteredCloudAdvocateContributions).ConfigureAwait(false);

            return response;
        }
    }

    static class DateTimeExtensions
    {
        public static bool IsWithinRange(this DateTimeOffset dateTimeOffset, DateTimeOffset start, DateTimeOffset end) => dateTimeOffset >= start && dateTimeOffset <= end;
    }
}
