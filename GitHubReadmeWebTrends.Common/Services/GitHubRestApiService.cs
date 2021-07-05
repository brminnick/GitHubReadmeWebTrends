using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace GitHubReadmeWebTrends.Common
{
    public class GitHubRestApiService
    {
        readonly IGitHubRestApiClient _gitHubApiClient;

        public GitHubRestApiService(IGitHubRestApiClient gitHubApiClient) => _gitHubApiClient = gitHubApiClient;

        public Task<RepositoryFile> GetReadme(string repositoryOwner, string repositoryName) => _gitHubApiClient.GetReadme(repositoryOwner, repositoryName);
        public Task<RepositoryFile> GetFile(string repositoryOwner, string repositoryName, string filePath, string branchName) => _gitHubApiClient.GetFile(repositoryOwner, repositoryName, filePath, branchName);
        public Task<CreateForkResponseModel> CreateFork(string gitHubUserName, string repositoryName) => _gitHubApiClient.CreateFork(gitHubUserName, repositoryName);
        public Task<UpdateFileResponseModel> UpdateFile(string repositoryOwner, string repositoryName, string filePath, UpdateFileContentModel updateFileContentMode) => _gitHubApiClient.UpdateFile(repositoryOwner, repositoryName, filePath, updateFileContentMode);

        public async Task DeleteRepository(string gitHubUserName, string repositoryName)
        {
            var response = await _gitHubApiClient.DeleteRepository(gitHubUserName, repositoryName).ConfigureAwait(false);
            if (response.StatusCode != System.Net.HttpStatusCode.NoContent)
                throw new System.Exception("Failed to Delete Repository");
        }

        Task<IReadOnlyList<RepositoryFile>> GetAllAdvocateFiles() => _gitHubApiClient.GetAllAdvocateFiles();
        Task<RepositoryFile> OpenPullRequest(string gitHubUserName, string repositoryName) => _gitHubApiClient.OpenPullRequest(gitHubUserName, repositoryName);
        Task<HttpResponseMessage> GetResponseMessage() => _gitHubApiClient.GetGitHubApiResponse();
    }
}
