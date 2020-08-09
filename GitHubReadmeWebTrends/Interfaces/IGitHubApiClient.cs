using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Refit;

namespace VerifyGitHubReadmeLinks
{
    [Headers("User-Agent: " + nameof(VerifyGitHubReadmeLinks), "Accept-Encoding: gzip", "Accept: application/json")]
    interface IGitHubApiClient
    {
        [Get("/repos/MicrosoftDocs/cloud-developer-advocates/contents/advocates")]
        public Task<List<RepositoryFile>> GetAllAdvocateFiles();

        [Get("/repos/{owner}/{repo}/readme")]
        public Task<RepositoryFile> GetReadme([AliasAs("owner")] string gitHubUserName, [AliasAs("repo")] string repositoryName);

        [Get("/repos/{owner}/{repo}/contents/{path}")]
        public Task<RepositoryFile> GetFile([AliasAs("owner")] string gitHubUserName, [AliasAs("repo")] string repositoryName, [AliasAs("path")] string filePath, [AliasAs("ref")] string branchName);

        [Put("/repos/{owner}/{repo}/contents/{path}")]
        public Task<UpdateFileResponseModel> UpdateFile([AliasAs("owner")] string gitHubUserName, [AliasAs("repo")] string repositoryName, [AliasAs("path")] string filePath, [Body] UpdateFileContentModel updateFileContentModel);

        [Post("/repos/{owner}/{repo}/pulls")]
        public Task<RepositoryFile> OpenPullRequest([AliasAs("owner")] string gitHubUserName, [AliasAs("repo")] string repositoryName);

        [Post("/repos/{owner}/{repo}/forks")]
        public Task<CreateForkResponseModel> CreateFork([AliasAs("owner")] string gitHubUserName, [AliasAs("repo")] string repositoryName);
    }
}
