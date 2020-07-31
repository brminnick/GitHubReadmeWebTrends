using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace VerifyGitHubReadmeLinks
{
    class GetAdvocatesFunction
    {
        const string _runOncePerMonth = "0 0 5 * * *";

        readonly HttpClient _httpClient;
        readonly YamlService _yamlService;
        readonly GitHubApiService _gitHubApiService;

        public GetAdvocatesFunction(GitHubApiService gitHubApiService, YamlService yamlService, IHttpClientFactory httpClientFactory) =>
            (_gitHubApiService, _yamlService, _httpClient) = (gitHubApiService, yamlService, httpClientFactory.CreateClient());

        [FunctionName(nameof(GetAdvocatesFunction))]
        public async Task Run([TimerTrigger(_runOncePerMonth, RunOnStartup = true)] TimerInfo myTimer, ILogger log,
            [Queue(QueueConstants.GetAdvocatesQueue)] ICollector<GitHubUserModel> advocateModels)
        {
            log.LogInformation($"{nameof(GetAdvocatesFunction)} Started");

            await foreach (var gitHubUser in GetAzureAdvocates().ConfigureAwait(false))
            {
                advocateModels.Add(gitHubUser);
            }

            log.LogInformation($"Completed");
        }

        async IAsyncEnumerable<GitHubUserModel> GetAzureAdvocates()
        {
            await foreach (var file in GetYamlFiles().ConfigureAwait(false))
            {
                var advocate = _yamlService.ParseAdvocateFromYaml(file);
                if (advocate != null)
                    yield return advocate;
            }
        }

        async IAsyncEnumerable<string> GetYamlFiles()
        {
            var azureAdvocateRepositoryFiles = await _gitHubApiService.GetAllAdvocateFiles().ConfigureAwait(false);
            PrintRepositoryUrls(azureAdvocateRepositoryFiles);

            var downloadFileTaskList = azureAdvocateRepositoryFiles.Where(x => x.DownloadUrl != null).Select(x => _httpClient.GetStringAsync(x.DownloadUrl)).ToList();

            while (downloadFileTaskList.Any())
            {
                var downloadFileTask = await Task.WhenAny(downloadFileTaskList).ConfigureAwait(false);
                downloadFileTaskList.Remove(downloadFileTask);

                var file = await downloadFileTask.ConfigureAwait(false);

                if (file != null)
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