using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace VerifyGitHubReadmeLinks
{
    class GitHubApiService
    {
        readonly static IDeserializer _yamlDeserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();

        readonly HttpClient _httpClient;

        public GitHubApiService(IHttpClientFactory httpClientFactory) => _httpClient = httpClientFactory.CreateClient(nameof(GitHubApiService));


        public async Task<List<RepositoryFile>> GetAllAdvocateFiles()
        {
            var response = await _httpClient.GetAsync($"{GitHubConstants.GitHubRestApiUrl}/repos/MicrosoftDocs/cloud-developer-advocates/contents/advocates").ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<List<RepositoryFile>>(jsonContent);
        }

        public async IAsyncEnumerable<GitHubUserModel> GetAzureAdvocates()
        {
            await foreach (var file in GetYamlFiles().ConfigureAwait(false))
            {
                yield return ParseAdvocateFromYaml(file);
            }
        }

        async IAsyncEnumerable<string> GetYamlFiles()
        {
            var azureAdvocateRepositoryFiles = await GetAllAdvocateFiles().ConfigureAwait(false);
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

        GitHubUserModel ParseAdvocateFromYaml(in string file)
        {
            const string gitHubDomain = "github.com/";

            var stringReaderFile = new StringReader(file);
            var cloudAdvocate = _yamlDeserializer.Deserialize<CloudAdvocateYamlModel>(stringReaderFile);

            var fullName = cloudAdvocate.Name;

            var gitHubUrl = cloudAdvocate.Connect.First(x => x.Title.Contains("GitHub", StringComparison.OrdinalIgnoreCase)).Url;
            var gitHubUserName = parseGitHubUserNameFromUrl(gitHubUrl.ToString());

            return new GitHubUserModel(fullName, gitHubUserName);

            static string parseGitHubUserNameFromUrl(in string gitHubUrl)
            {
                var indexOfGitHubDomain = gitHubUrl.LastIndexOf(gitHubDomain);
                var indexOfGitHubUserName = indexOfGitHubDomain + gitHubDomain.Length;

                return gitHubUrl.Substring(indexOfGitHubUserName).Trim('/');
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
