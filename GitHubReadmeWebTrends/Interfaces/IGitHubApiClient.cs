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

        [Post("/repos/{owner}/{repo}/pulls")]
        public Task<RepositoryFile> OpenPullRequest([AliasAs("owner")] string gitHubUserName, [AliasAs("repo")] string repositoryName);

        [Get("/repos/{owner}/{repo}/git/{prefix1}/{prefix2}/{branchName}")]
        public Task<GitHubReferenceModel> GetDefaultBranchRefrence([AliasAs("owner")] string gitHubUserName, [AliasAs("repo")] string repositoryName, string prefix1, string prefix2, string branchName);
    }
}
