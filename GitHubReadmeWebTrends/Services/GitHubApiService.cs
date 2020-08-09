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
        public Task<RepositoryFile> OpenPullRequest(string gitHubUserName, string repositoryName) => _gitHubApiClient.OpenPullRequest(gitHubUserName, repositoryName);
        public Task<CreateForkResponseModel> CreateFork(string gitHubUserName, string repositoryName) => _gitHubApiClient.CreateFork(gitHubUserName, repositoryName);
        public Task<RepositoryFile> GetFile(string repositoryOwner, string repositoryName, string filePath, string branchName) => _gitHubApiClient.GetFile(repositoryOwner, repositoryName, filePath, branchName);
        public Task<RepositoryFile> UpdateFile(string repositoryOwner, string repositoryName, string filePath, string branchName, UpdateFileContentModel updateFileContentMode) => _gitHubApiClient.UpdateFile(repositoryOwner, repositoryName, filePath, branchName, updateFileContentMode);
    }
}
