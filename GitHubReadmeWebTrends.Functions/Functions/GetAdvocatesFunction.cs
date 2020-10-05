using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GitHubReadmeWebTrends.Common;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GitHubReadmeWebTrends.Functions
{
    class GetAdvocatesFunction
    {
        const string _runOncePerMonth = "0 0 0 5 * *";

        const bool _shouldRunOnStartup =
#if DEBUG
            true;
#else
            false;
#endif

#if DEBUG
        readonly static IReadOnlyList<string> _betaTesterAliases = new[] { "bramin", "shboyer", "sicotin", "jopapa", "masoucou" };
#endif

        readonly YamlService _yamlService;
        readonly OptOutDatabase _optOutDatabase;
        readonly CloudAdvocateYamlService _cloudAdvocateYamlService;

        public GetAdvocatesFunction(YamlService yamlService, CloudAdvocateYamlService cloudAdvocateYamlService, OptOutDatabase optOutDatabase)
        {
            _yamlService = yamlService;
            _optOutDatabase = optOutDatabase;
            _cloudAdvocateYamlService = cloudAdvocateYamlService;
        }

        [FunctionName(nameof(GetAdvocatesFunction))]
        public async Task RunTimerTrigger([TimerTrigger(_runOncePerMonth, RunOnStartup = _shouldRunOnStartup)] TimerInfo myTimer, ILogger log,
                                [Queue(QueueConstants.AdvocatesQueue)] ICollector<CloudAdvocateGitHubUserModel> advocateModels)
        {
            log.LogInformation($"{nameof(GetAdvocatesFunction)} Started");

            var optOutList = _optOutDatabase.GetAllOptOutModels();

            await foreach (var gitHubUser in _cloudAdvocateYamlService.GetAzureAdvocates().ConfigureAwait(false))
            {
#if DEBUG
                if (!_betaTesterAliases.Contains(gitHubUser.MicrosoftAlias))
                    continue;

                log.LogInformation($"Beta Tester Found: {gitHubUser.MicrosoftAlias}");
#endif

                var matchingOptOutModel = optOutList.SingleOrDefault(x => x.Alias == gitHubUser.MicrosoftAlias);

                // Only add users who have not opted out
                // `null` indicates that the user has never used GitHubReadmeWebTrends.Website 
                if (matchingOptOutModel is null || !matchingOptOutModel.HasOptedOut)
                    advocateModels.Add(gitHubUser);
            }

            log.LogInformation($"Completed");
        }

        [FunctionName(nameof(GetFriendsTimerTrigger))]
        public async Task GetFriendsTimerTrigger([TimerTrigger(_runOncePerMonth, RunOnStartup = _shouldRunOnStartup)] TimerInfo myTimer, ILogger log,
                                [Queue(QueueConstants.AdvocatesQueue)] ICollector<CloudAdvocateGitHubUserModel> advocateModels)
        {
            log.LogInformation($"{nameof(GetFriendsTimerTrigger)} Started");

            var client = new HttpClient();
            var json = await client.GetStringAsync("https://gist.githubusercontent.com/jamesmontemagno/f4d3986c91867449153c389cc31d0abc/raw/6576587b1300d47f41ffa7a627ac37cdc6aa0822/team.json");

            var friends = JsonConvert.DeserializeObject<CloudAdvocateGitHubUserModel[]>(json);

            var optOutList = _optOutDatabase.GetAllOptOutModels();

            foreach (var gitHubUser in friends)
            {
#if DEBUG
                if (!_betaTesterAliases.Contains(gitHubUser.MicrosoftAlias))
                    continue;

                log.LogInformation($"Beta Tester Found: {gitHubUser.MicrosoftAlias}");
#endif

                var matchingOptOutModel = optOutList.SingleOrDefault(x => x.Alias == gitHubUser.MicrosoftAlias);

                // Only add users who have not opted out
                // `null` indicates that the user has never used GitHubReadmeWebTrends.Website 
                if (matchingOptOutModel is null || !matchingOptOutModel.HasOptedOut)
                    advocateModels.Add(gitHubUser);
            }

            log.LogInformation($"Completed");
        }

        [Conditional("DEBUG")]
        void PrintRepositoryUrls(in IEnumerable<RepositoryFile> repositoryFiles)
        {
            foreach (var repository in repositoryFiles)
            {
                Debug.WriteLine($"File Name: {repository.FileName}");
                Debug.WriteLine($"Download Url: {repository.DownloadUrl?.ToString() ?? "null"}");
                Debug.WriteLine("");
            }
        }
    }
}