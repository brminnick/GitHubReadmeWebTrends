using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitHubReadmeWebTrends.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace GitHubReadmeWebTrends.Functions
{
    public class GetAdvocatesFunction
    {
        const string _runOncePerMonth = "0 0 0 5 * *";

        const bool _shouldRunOnStartup =
#if DEBUG
            true;
#else
            false;
#endif

        readonly static IReadOnlyList<string> _betaTesterAliases = new[] { "bramin" };

        readonly OptOutDatabase _optOutDatabase;
        readonly AdvocateService _advocateService;

        public GetAdvocatesFunction(AdvocateService advocateService, OptOutDatabase optOutDatabase)
        {
            _optOutDatabase = optOutDatabase;
            _advocateService = advocateService;
        }

        [Function(nameof(GetAzureAdvocatesBetaTestersTimerTrigger)), QueueOutput(QueueConstants.AdvocatesQueue)]
        public async Task<IReadOnlyList<AdvocateModel>> GetAzureAdvocatesBetaTestersTimerTrigger([TimerTrigger(_runOncePerMonth, RunOnStartup = _shouldRunOnStartup)] TimerInfo myTimer, FunctionContext context)
        {
            var log = context.GetLogger<GetAdvocatesFunction>();
            log.LogInformation($"{nameof(GetAzureAdvocatesBetaTestersTimerTrigger)} Started");

            var advocateModels = new List<AdvocateModel>();
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            var optOutList = await _optOutDatabase.GetAllOptOutModels().ConfigureAwait(false);

            var currentAdvocateList = await _advocateService.GetCurrentAdvocates(cancellationTokenSource.Token).ConfigureAwait(false);

            foreach (var advocate in currentAdvocateList)
            {

                if (!IsBetaTester(advocate))
                    continue;

                log.LogInformation($"Beta Tester Found: {advocate.MicrosoftAlias}");

                if (!HasUserOptedOut(advocate, optOutList))
                    advocateModels.Add(advocate);
            }

            log.LogInformation($"{nameof(GetAzureAdvocatesBetaTestersTimerTrigger)} Completed");

            return advocateModels;
        }

        [FunctionName(nameof(GetAzureAdvocatesTimerTrigger))]
        public async Task GetAzureAdvocatesTimerTrigger([TimerTrigger(_runOncePerMonth)] TimerInfo myTimer, ILogger log,
                                [Queue(QueueConstants.AdvocatesQueue)] ICollector<AdvocateModel> advocateModels)
        {
            log.LogInformation($"{nameof(GetAzureAdvocatesTimerTrigger)} Started");

            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            var optOutList = await _optOutDatabase.GetAllOptOutModels().ConfigureAwait(false);

            var currentAdvocateList = await _advocateService.GetCurrentAdvocates(cancellationTokenSource.Token).ConfigureAwait(false);

            foreach (var advocate in currentAdvocateList)
            {
                if (!HasUserOptedOut(advocate, optOutList))
                    advocateModels.Add(advocate);
            }

            log.LogInformation($"{nameof(GetAzureAdvocatesTimerTrigger)} Completed");
        }

        [FunctionName(nameof(GetFriendsTimerTrigger))]
        public async Task GetFriendsTimerTrigger([TimerTrigger(_runOncePerMonth)] TimerInfo myTimer, ILogger log,
                                [Queue(QueueConstants.AdvocatesQueue)] ICollector<AdvocateModel> advocateModels)
        {
            log.LogInformation($"{nameof(GetFriendsTimerTrigger)} Started");

            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            var friendsOfAzureList = await _advocateService.GetFriendsOfAdvocates().ConfigureAwait(false);

            foreach (var gitHubUser in friendsOfAzureList)
            {
                advocateModels.Add(gitHubUser);
            }

            log.LogInformation($"{nameof(GetFriendsTimerTrigger)} Completed");
        }

        bool IsBetaTester(AdvocateModel cloudAdvocateGitHubUserModel) => _betaTesterAliases.Contains(cloudAdvocateGitHubUserModel.MicrosoftAlias);

        bool HasUserOptedOut(AdvocateModel advocateModel, IReadOnlyList<OptOutModel> optOutUserModels)
        {
            var matchingOptOutModel = optOutUserModels.SingleOrDefault(x => x.Alias == advocateModel.MicrosoftAlias);

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