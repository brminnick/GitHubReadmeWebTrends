using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace AzureAdvocates.Functions
{
    class GetMicrosoftLearnContributors
    {
        readonly BlobStorageService _blobStorageService;

        public GetMicrosoftLearnContributors(BlobStorageService blobStorageService) => _blobStorageService = blobStorageService;

        [FunctionName(nameof(GetMicrosoftLearnContributors))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = nameof(GetMicrosoftLearnContributors) + "/{from:datetime}/{to:datetime}/{team?}")] HttpRequestMessage req, DateTime from, DateTime to, string? team, ILogger log)
        {
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

            return new OkObjectResult(filteredCloudAdvocateContributions);
        }
    }

    static class DateTimeExtensions
    {
        public static bool IsWithinRange(this DateTimeOffset dateTimeOffset, DateTimeOffset start, DateTimeOffset end) => dateTimeOffset >= start && dateTimeOffset <= end;
    }
}
