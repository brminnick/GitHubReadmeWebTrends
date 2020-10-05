using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GitHubReadmeWebTrends.Common;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

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
        readonly static IReadOnlyList<string> _betaTesterAliases = new[] { "bramin", "shboyer", "sicotin", "jopapa", "masoucou", "jamont", "v-gmohapi", "judubois", "ropreddy", "sakriema", "juyoo", "mijam", "ninarasi" };
#endif

        readonly YamlService _yamlService;
        readonly OptOutDatabase _optOutDatabase;
        readonly CloudAdvocateService _cloudAdvocateService;

        public GetAdvocatesFunction(YamlService yamlService, CloudAdvocateService cloudAdvocateService, OptOutDatabase optOutDatabase)
        {
            _yamlService = yamlService;
            _optOutDatabase = optOutDatabase;
            _cloudAdvocateService = cloudAdvocateService;
        }

        [FunctionName(nameof(GetAzureAdvocatesTimerTrigger))]
        public async Task GetAzureAdvocatesTimerTrigger([TimerTrigger(_runOncePerMonth, RunOnStartup = false)] TimerInfo myTimer, ILogger log,
                                [Queue(QueueConstants.AdvocatesQueue)] ICollector<CloudAdvocateGitHubUserModel> advocateModels)
        {
            log.LogInformation($"{nameof(GetAdvocatesFunction)} Started");

            var optOutList = _optOutDatabase.GetAllOptOutModels();

            await foreach (var gitHubUser in _cloudAdvocateService.GetAzureAdvocates().ConfigureAwait(false))
            {
#if DEBUG
                if (!IsBetaTester(gitHubUser))
                    continue;

                log.LogInformation($"Beta Tester Found: {gitHubUser.MicrosoftAlias}");
#endif

                if (!HasUserOptedOut(gitHubUser))
                    advocateModels.Add(gitHubUser);
            }

            log.LogInformation($"Completed");
        }

        [FunctionName(nameof(GetFriendsTimerTrigger))]
        public async Task GetFriendsTimerTrigger([TimerTrigger(_runOncePerMonth, RunOnStartup = _shouldRunOnStartup)] TimerInfo myTimer, ILogger log,
                                [Queue(QueueConstants.AdvocatesQueue)] ICollector<CloudAdvocateGitHubUserModel> advocateModels)
        {
            log.LogInformation($"{nameof(GetFriendsTimerTrigger)} Started");

            var friendsOfAzureList = await _cloudAdvocateService.GetFriendsOfAdvocates().ConfigureAwait(false);

            foreach (var gitHubUser in friendsOfAzureList)
            {
#if DEBUG
                if (!IsBetaTester(gitHubUser))
                    continue;

                log.LogInformation($"Beta Tester Found: {gitHubUser.MicrosoftAlias}");
#endif
                advocateModels.Add(gitHubUser);
            }

            log.LogInformation($"Completed");
        }

#if DEBUG
        bool IsBetaTester(CloudAdvocateGitHubUserModel cloudAdvocateGitHubUserModel) => _betaTesterAliases.Contains(cloudAdvocateGitHubUserModel.MicrosoftAlias);
#endif

        bool HasUserOptedOut(CloudAdvocateGitHubUserModel cloudAdvocateGitHubUserModel)
        {
            var optOutList = _optOutDatabase.GetAllOptOutModels();

            var matchingOptOutModel = optOutList.SingleOrDefault(x => x.Alias == cloudAdvocateGitHubUserModel.MicrosoftAlias);

            // `null` indicates that the user has never opted out by using the GitHubReadmeWebTrends.Website 
            return matchingOptOutModel?.HasOptedOut ?? false;
        }

        [Conditional("DEBUG")]
        static void PrintRepositoryUrls(in IEnumerable<RepositoryFile> repositoryFiles)
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