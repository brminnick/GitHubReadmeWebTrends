using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GitHubReadmeWebTrends.Common;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace GitHubReadmeWebTrends.Functions
{
    class GetAdvocatesForPowerBIDashboard
    {
        readonly YamlService _yamlService;
        readonly CloudAdvocateYamlService _cloudAdvocateYamlService;

        public GetAdvocatesForPowerBIDashboard(CloudAdvocateYamlService cloudAdvocateYamlService, YamlService yamlService) =>
            (_cloudAdvocateYamlService, _yamlService) = (cloudAdvocateYamlService, yamlService);

        [FunctionName(nameof(GetAdvocatesForPowerBIDashboard))]
        public async IAsyncEnumerable<CloudAdvocatePowerBIModel> RunHttpTrigger([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestMessage req, ILogger log)
        {
            log.LogInformation($"{nameof(GetAdvocatesForPowerBIDashboard)} Started");

            await foreach (var yamlFile in _cloudAdvocateYamlService.GetCloudAdvocateYamlFiles().ConfigureAwait(false))
            {
                var advocate = _yamlService.ParseCloudAdvocateModelFromYaml(yamlFile, log);
                if (advocate is null)
                    continue;

                var gitHubUri = advocate.Connect.SingleOrDefault(x => x.Title.Contains("GitHub", StringComparison.OrdinalIgnoreCase))?.Url ?? throw new Exception($"Missing GitHub Uri for {advocate.Name}");
                var twitterUri = advocate.Connect.SingleOrDefault(x => x.Title.Contains("Twitter", StringComparison.OrdinalIgnoreCase))?.Url ?? throw new Exception($"Missing Twitter Uri for {advocate.Name}");
                var linkedInUri = advocate.Connect.SingleOrDefault(x => x.Title.Contains("LinkedIn", StringComparison.OrdinalIgnoreCase))?.Url ?? throw new Exception($"Missing LinkedIn Uri for {advocate.Name}");

                yield return new CloudAdvocatePowerBIModel(advocate.Name, advocate.Metadata.Alias, gitHubUri, twitterUri, linkedInUri);
            }

            log.LogInformation($"Completed");
        }
    }
}
