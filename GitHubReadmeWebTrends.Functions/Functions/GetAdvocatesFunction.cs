using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GitHubReadmeWebTrends.Common;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace GitHubReadmeWebTrends.Functions
{
    partial class GetAdvocatesFunction
    {
        const string _runOncePerMonth = "0 0 0 5 * *";

        const bool _shouldRunOnStartup =
#if DEBUG
           false;// true;
#else
            false;
#endif

#if DEBUG
        readonly static IReadOnlyList<string> _betaTesterAliases = new[] { "bramin", "shboyer", "sicotin", "jopapa", "masoucou" };
#endif

        readonly HttpClient _httpClient;
        readonly YamlService _yamlService;
        readonly OptOutDatabase _optOutDatabase;
        readonly GitHubApiService _gitHubApiService;

        public GetAdvocatesFunction(GitHubApiService gitHubApiService, YamlService yamlService, IHttpClientFactory httpClientFactory, OptOutDatabase optOutDatabase)
        {
            _yamlService = yamlService;
            _optOutDatabase = optOutDatabase;
            _gitHubApiService = gitHubApiService;
            _httpClient = httpClientFactory.CreateClient();
        }

        [FunctionName(nameof(GetAdvocatesFunction))]
        public async Task RunTimerTrigger([TimerTrigger(_runOncePerMonth, RunOnStartup = _shouldRunOnStartup)] TimerInfo myTimer, ILogger log,
                                [Queue(QueueConstants.AdvocatesQueue)] ICollector<CloudAdvocateGitHubUserModel> advocateModels)
        {
            log.LogInformation($"{nameof(GetAdvocatesFunction)} Started");

            var optOutList = _optOutDatabase.GetAllOptOutModels();

            await foreach (var gitHubUser in GetAzureAdvocates(log).ConfigureAwait(false))
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

        async IAsyncEnumerable<CloudAdvocateGitHubUserModel> GetAzureAdvocates(ILogger log)
        {
            await foreach (var file in GetYamlFiles().ConfigureAwait(false))
            {
                var advocate = _yamlService.ParseCloudAdvocateGitHubUserModelFromYaml(file, log);

                if (advocate is null || string.IsNullOrWhiteSpace(advocate.FullName))
                    log.LogError($"Invalid GitHub Url\n{file}\n");
                else if (string.IsNullOrWhiteSpace(advocate.GitHubUserName))
                    log.LogError($"Invalid GitHub UserName for {advocate.FullName}");
                else
                    yield return advocate;
            }
        }

        async IAsyncEnumerable<string> GetYamlFiles()
        {
            var azureAdvocateRepositoryFiles = await _gitHubApiService.GetAllAdvocateFiles().ConfigureAwait(false);

            var downloadFileTaskList = azureAdvocateRepositoryFiles.Where(x => x.DownloadUrl != null).Select(x => _httpClient.GetStringAsync(x.DownloadUrl)).ToList();

            while (downloadFileTaskList.Any())
            {
                var downloadFileTask = await Task.WhenAny(downloadFileTaskList).ConfigureAwait(false);
                downloadFileTaskList.Remove(downloadFileTask);

                var file = await downloadFileTask.ConfigureAwait(false);

                if (file != null && file.StartsWith("### YamlMime:Profile") && !file.StartsWith("### YamlMime:ProfileList"))
                    yield return file;
            }
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