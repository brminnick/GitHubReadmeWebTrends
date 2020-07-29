using System;
using System.Collections.Generic;
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

        public GitHubApiService(HttpClient httpClient) => _httpClient = httpClient;


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
            var azureAdvocateFiles = await GetAllAdvocateFiles().ConfigureAwait(false);

            var downloadFileTaskList = azureAdvocateFiles.Select(x => _httpClient.GetStringAsync(x.DownloadUrl));

            while (downloadFileTaskList.Any())
            {
                var downloadFileTask = await Task.WhenAny(downloadFileTaskList).ConfigureAwait(false);
                var file = await downloadFileTask.ConfigureAwait(false);

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
    }
}
