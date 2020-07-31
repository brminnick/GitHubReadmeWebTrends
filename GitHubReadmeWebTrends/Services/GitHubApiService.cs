using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;

namespace VerifyGitHubReadmeLinks
{
    class GitHubApiService
    {
        readonly IGitHubApiClient _gitHubApiClient;

        public GitHubApiService(IGitHubApiClient gitHubApiClient) => _gitHubApiClient = gitHubApiClient;

        public Task<List<RepositoryFile>> GetAllAdvocateFiles() => _gitHubApiClient.GetAllAdvocateFiles();
    }

    [Headers("User-Agent: " + nameof(GitHubApiService), "Accept-Encoding: gzip", "Accept: application/json")]
    interface IGitHubApiClient
    {
        [Get("/repos/MicrosoftDocs/cloud-developer-advocates/contents/advocates")]
        public Task<List<RepositoryFile>> GetAllAdvocateFiles();
    }
}
