using System.Collections.Generic;
using System.Threading.Tasks;

namespace VerifyGitHubReadmeLinks
{
    class GitHubApiService
    {
        readonly IGitHubApiClient _gitHubApiClient;

        public GitHubApiService(IGitHubApiClient gitHubApiClient) => _gitHubApiClient = gitHubApiClient;

        public Task<List<RepositoryFile>> GetAllAdvocateFiles() => _gitHubApiClient.GetAllAdvocateFiles();
        public Task<RepositoryFile> GetReadme(string repositoryOwner, string repositoryName) => _gitHubApiClient.GetReadme(repositoryOwner, repositoryName);
    }
}
