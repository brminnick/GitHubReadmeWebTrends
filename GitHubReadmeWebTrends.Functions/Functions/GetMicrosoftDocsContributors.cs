using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using GitHubReadmeWebTrends.Common;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace GitHubReadmeWebTrends.Functions
{
    class GetMicrosoftDocsContributors
    {
        readonly CloudAdvocateService _cloudAdvocateService;
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;

        public GetMicrosoftDocsContributors(CloudAdvocateService cloudAdvocateService, GitHubGraphQLApiService gitHubGraphQLApiService) =>
            (_cloudAdvocateService, _gitHubGraphQLApiService) = (cloudAdvocateService, gitHubGraphQLApiService);

        [FunctionName(nameof(GetMicrosoftDocsContributors))]
        public async IAsyncEnumerable<GitHubContributorModel> RunHttpTrigger([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestMessage req, DateTimeOffset from, DateTimeOffset to, ILogger log)
        {
            log.LogInformation($"{nameof(GetMicrosoftDocsContributors)} Started");

            await foreach (var cloudAdvocateGitHubUserModel in _cloudAdvocateService.GetAzureAdvocates().ConfigureAwait(false))
            {
                var contributions = await _gitHubGraphQLApiService.GetMicrosoftDocsContributionsCollection(cloudAdvocateGitHubUserModel.GitHubUserName, from, to).ConfigureAwait(false);
                yield return new GitHubContributorModel(contributions, cloudAdvocateGitHubUserModel);
            }
        }
    }
}
