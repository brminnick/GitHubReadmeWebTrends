using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GitHubReadmeWebTrends.Common
{
    public class CloudAdvocateService
    {
        readonly ILogger _logger;
        readonly HttpClient _httpClient;
        readonly YamlService _yamlService;
        readonly GitHubApiService _gitHubApiService;

        public CloudAdvocateService(GitHubApiService gitHubApiService, YamlService yamlService, IHttpClientFactory httpClientFactory, ILogger<CloudAdvocateService> logger)
        {
            _logger = logger;
            _yamlService = yamlService;
            _gitHubApiService = gitHubApiService;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async IAsyncEnumerable<string> GetCloudAdvocateYamlFiles()
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

        public async IAsyncEnumerable<CloudAdvocateGitHubUserModel> GetAzureAdvocates()
        {
            await foreach (var file in GetCloudAdvocateYamlFiles().ConfigureAwait(false))
            {
                var advocate = _yamlService.ParseCloudAdvocateGitHubUserModelFromYaml(file, _logger);

                if (advocate is null || string.IsNullOrWhiteSpace(advocate.FullName))
                    _logger.LogError($"Invalid GitHub Url\n{file}\n");
                else if (string.IsNullOrWhiteSpace(advocate.GitHubUserName))
                    _logger.LogError($"Invalid GitHub UserName for {advocate.FullName}");
                else
                    yield return advocate;
            }
        }

        public async Task<IReadOnlyList<CloudAdvocateGitHubUserModel>> GetFriendsOfAdvocates()
        {
            var json = await _httpClient.GetStringAsync("https://gist.githubusercontent.com/jamesmontemagno/f4d3986c91867449153c389cc31d0abc/raw/6576587b1300d47f41ffa7a627ac37cdc6aa0822/team.json");

            return JsonConvert.DeserializeObject<CloudAdvocateGitHubUserModel[]>(json);
        }
    }
}