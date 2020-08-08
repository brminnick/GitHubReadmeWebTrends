using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace VerifyGitHubReadmeLinks
{
    class GitHubApiService
    {
        readonly IGitHubApiClient _gitHubApiClient;

        public GitHubApiService(IGitHubApiClient gitHubApiClient) => _gitHubApiClient = gitHubApiClient;

        public Task<List<RepositoryFile>> GetAllAdvocateFiles() => _gitHubApiClient.GetAllAdvocateFiles();
        public Task<RepositoryFile> GetReadme(string repositoryOwner, string repositoryName) => _gitHubApiClient.GetReadme(repositoryOwner, repositoryName);
        public Task<RepositoryFile> OpenPullRequest(string gitHubUserName, string repositoryName) => _gitHubApiClient.OpenPullRequest(gitHubUserName, repositoryName);
        public Task<GitHubReferenceModel> GetDefaultBranchRefrence(string gitHubUserName, string repositoryName, string prefix1, string prefix2, string branchName) => _gitHubApiClient.GetDefaultBranchRefrence(gitHubUserName, repositoryName, prefix1, prefix2, branchName);
    }
}
