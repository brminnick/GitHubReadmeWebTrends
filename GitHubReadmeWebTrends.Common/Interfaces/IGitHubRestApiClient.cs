using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Refit;

namespace GitHubReadmeWebTrends.Common
{
    [Headers("User-Agent: " + nameof(GitHubReadmeWebTrends), "Accept-Encoding: gzip", "Accept: application/json")]
    public interface IGitHubRestApiClient
    {
        [Get("/repos/MicrosoftDocs/cloud-developer-advocates/contents/advocates")]
        Task<IReadOnlyList<RepositoryFile>> GetAllAdvocateFiles();

        [Get("/repos/{owner}/{repo}/readme")]
        Task<RepositoryFile> GetReadme([AliasAs("owner")] string gitHubUserName, [AliasAs("repo")] string repositoryName);

        [Get("/repos/{owner}/{repo}/contents/{path}")]
        Task<RepositoryFile> GetFile([AliasAs("owner")] string gitHubUserName, [AliasAs("repo")] string repositoryName, [AliasAs("path")] string filePath, [AliasAs("ref")] string branchName);

        [Put("/repos/{owner}/{repo}/contents/{path}")]
        Task<UpdateFileResponseModel> UpdateFile([AliasAs("owner")] string gitHubUserName, [AliasAs("repo")] string repositoryName, [AliasAs("path")] string filePath, [Body] UpdateFileContentModel updateFileContentModel);

        [Post("/repos/{owner}/{repo}/pulls")]
        Task<RepositoryFile> OpenPullRequest([AliasAs("owner")] string gitHubUserName, [AliasAs("repo")] string repositoryName);

        [Post("/repos/{owner}/{repo}/forks")]
        Task<CreateForkResponseModel> CreateFork([AliasAs("owner")] string gitHubUserName, [AliasAs("repo")] string repositoryName);

        [Delete("/repos/{owner}/{repo}")]
        Task<HttpResponseMessage> DeleteRepository([AliasAs("owner")] string gitHubUserName, [AliasAs("repo")] string repositoryName);

        [Get("/repos/brminnick/GitHubReadmeWebTrends")]
        Task<HttpResponseMessage> GetGitHubApiResponse();
    }
}
