using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = nameof(GetMicrosoftLearnContributors) + "/{fromDateTime}/{toDateTime}/{team?}")] HttpRequestData req, string fromDateTime, string toDateTime, string? team, FunctionContext context)
        {
            var log = context.GetLogger<GetMicrosoftLearnContributors>();
            log.LogInformation($"{nameof(GetMicrosoftLearnContributors)} Started");

            var isFromValid = DateTime.TryParse(fromDateTime, out var from);
            var isToValid = DateTime.TryParse(toDateTime, out var to);

            if (!isFromValid || !isToValid)
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Invalid Dates Provided").ConfigureAwait(false);

                return badRequestResponse;
            }

            var microsoftLearnContributionsList = await _blobStorageService.GetCloudAdvocateMicrosoftLearnContributors().ConfigureAwait(false);

            var filteredCloudAdvocateContributions = new List<CloudAdvocateGitHubContributorModel>();
            foreach (var advocateContribution in microsoftLearnContributionsList)
            {
                if (team is null || advocateContribution.Team.Equals(team, StringComparison.OrdinalIgnoreCase))
                {
                    log.LogInformation($"Adding Advocate: {advocateContribution.Name}");

                    var filteredPullRequests = advocateContribution.PullRequests.Where(x => x.CreatedAt.IsWithinRange(from, to)).ToList();
                    var filteredCloudAdvocateContribution = new CloudAdvocateGitHubContributorModel(filteredPullRequests, advocateContribution.GitHubUsername, advocateContribution.MicrosoftAlias, advocateContribution.RedditUserName, advocateContribution.Team, advocateContribution.Name);

                    filteredCloudAdvocateContributions.Add(filteredCloudAdvocateContribution);
                }
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(filteredCloudAdvocateContributions).ConfigureAwait(false);

            return response;
        }
    }

    static class DateTimeExtensions
    {
        public static bool IsWithinRange(this DateTimeOffset dateTimeOffset, DateTimeOffset start, DateTimeOffset end) => dateTimeOffset >= start && dateTimeOffset <= end;
    }
}
