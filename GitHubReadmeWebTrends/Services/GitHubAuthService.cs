using System.Threading.Tasks;
using Refit;

namespace VerifyGitHubReadmeLinks
{
    class GitHubAuthService
    {
        readonly IGitHubAuthApi _gitHubAuthClient;

        public GitHubAuthService(IGitHubAuthApi gitHubAuthApi) => _gitHubAuthClient = gitHubAuthApi;

        public Task<GitHubToken> GetGitHubToken(string clientId, string clientSecret, string loginCode, string state) =>
            _gitHubAuthClient.GetAccessToken(clientId, clientSecret, loginCode, state);
    }

    [Headers("User-Agent: " + nameof(VerifyGitHubReadmeLinks), "Accept-Encoding: gzip", "Accept: application/json")]
    interface IGitHubAuthApi
    {
        [Get("/login/oauth/access_token")]
        Task<GitHubToken> GetAccessToken([AliasAs("client_id")] string clientId, [AliasAs("client_secret")] string clientSecret, [AliasAs("code")] string loginCode, [AliasAs("state")] string state);
    }
}
