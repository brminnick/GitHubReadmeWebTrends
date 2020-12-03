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
    public class GetAdvocatesFunction
    {
        const string _runOncePerMonth = "0 0 0 5 * *";

        const bool _shouldRunOnStartup = false;

        readonly static IReadOnlyList<string> _betaTesterAliases = new[] { "bramin", "shboyer", "sicotin", "jopapa", "masoucou", "jamont", "v-gmohapi", "judubois", "ropreddy", "sakriema", "juyoo", "mijam", "ninarasi" };

        readonly YamlService _yamlService;
        readonly OptOutDatabase _optOutDatabase;
        readonly CloudAdvocateService _cloudAdvocateService;

        public GetAdvocatesFunction(YamlService yamlService, CloudAdvocateService cloudAdvocateService, OptOutDatabase optOutDatabase)
        {
            _yamlService = yamlService;
            _optOutDatabase = optOutDatabase;
            _cloudAdvocateService = cloudAdvocateService;
        }

        [FunctionName(nameof(GetAzureAdvocatesBetaTestersTimerTrigger))]
        public async Task GetAzureAdvocatesBetaTestersTimerTrigger([TimerTrigger(_runOncePerMonth, RunOnStartup = _shouldRunOnStartup)] TimerInfo myTimer, ILogger log,
                        [Queue(QueueConstants.AdvocatesQueue)] ICollector<CloudAdvocateGitHubUserModel> advocateModels)
        {
            log.LogInformation($"{nameof(GetAzureAdvocatesBetaTestersTimerTrigger)} Started");

            var optOutList = _optOutDatabase.GetAllOptOutModels();

            await foreach (var gitHubUser in _cloudAdvocateService.GetAzureAdvocates().ConfigureAwait(false))
            {
                if (!IsBetaTester(gitHubUser))
                    continue;

                log.LogInformation($"Beta Tester Found: {gitHubUser.MicrosoftAlias}");

                if (!HasUserOptedOut(gitHubUser, optOutList))
                    advocateModels.Add(gitHubUser);
            }

            log.LogInformation($"{nameof(GetAzureAdvocatesBetaTestersTimerTrigger)} Completed");
        }

        [FunctionName(nameof(GetAzureAdvocatesTimerTrigger))]
        public async Task GetAzureAdvocatesTimerTrigger([TimerTrigger(_runOncePerMonth)] TimerInfo myTimer, ILogger log,
                                [Queue(QueueConstants.AdvocatesQueue)] ICollector<CloudAdvocateGitHubUserModel> advocateModels)
        {
            log.LogInformation($"{nameof(GetAzureAdvocatesTimerTrigger)} Started");

            var optOutList = _optOutDatabase.GetAllOptOutModels();

            await foreach (var gitHubUser in _cloudAdvocateService.GetAzureAdvocates().ConfigureAwait(false))
            {
                if (!HasUserOptedOut(gitHubUser, optOutList))
                    advocateModels.Add(gitHubUser);
            }

            log.LogInformation($"{nameof(GetAzureAdvocatesTimerTrigger)} Completed");
        }

        [FunctionName(nameof(GetFriendsTimerTrigger))]
        public async Task GetFriendsTimerTrigger([TimerTrigger(_runOncePerMonth)] TimerInfo myTimer, ILogger log,
                                [Queue(QueueConstants.AdvocatesQueue)] ICollector<CloudAdvocateGitHubUserModel> advocateModels)
        {
            log.LogInformation($"{nameof(GetFriendsTimerTrigger)} Started");

            var friendsOfAzureList = await _cloudAdvocateService.GetFriendsOfAdvocates().ConfigureAwait(false);

            foreach (var gitHubUser in friendsOfAzureList)
            {
                advocateModels.Add(gitHubUser);
            }

            log.LogInformation($"{nameof(GetFriendsTimerTrigger)} Completed");
        }

        bool IsBetaTester(CloudAdvocateGitHubUserModel cloudAdvocateGitHubUserModel) => _betaTesterAliases.Contains(cloudAdvocateGitHubUserModel.MicrosoftAlias);

        bool HasUserOptedOut(CloudAdvocateGitHubUserModel cloudAdvocateGitHubUserModel, IReadOnlyList<OptOutModel> optOutUserModels)
        {
            var matchingOptOutModel = optOutUserModels.SingleOrDefault(x => x.Alias == cloudAdvocateGitHubUserModel.MicrosoftAlias);

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