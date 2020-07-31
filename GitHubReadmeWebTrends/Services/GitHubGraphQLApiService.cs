using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VerifyGitHubReadmeLinks
{
    class GitHubGraphQLApiService
    {
        readonly IGitHubGraphQLApiClient _gitHubGraphQLApiClient;

        public GitHubGraphQLApiService(IGitHubGraphQLApiClient gitHubGraphQLApiClient) => _gitHubGraphQLApiClient = gitHubGraphQLApiClient;

        public async IAsyncEnumerable<IEnumerable<string>> GetRepositories(string repositoryOwner, int numberOfRepositoriesPerRequest = 100)
        {
            RepositoryConnectionResponse? repositoryConnection = null;

            do
            {
                repositoryConnection = await GetRepositoryConnectionResponse(repositoryOwner, repositoryConnection?.PageInfo?.EndCursor, numberOfRepositoriesPerRequest).ConfigureAwait(false);
                yield return repositoryConnection?.RepositoryList ?? Enumerable.Empty<string>();
            }
            while (repositoryConnection?.PageInfo?.HasNextPage is true);
        }

        async Task<RepositoryConnectionResponse> GetRepositoryConnectionResponse(string repositoryOwner, string? endCursor, int numberOfRepositoriesPerRequest = 100)
        {
            var response = await _gitHubGraphQLApiClient.RepositoryConnectionQuery(new RepositoryConnectionQueryContent(repositoryOwner, getEndCursorString(endCursor), numberOfRepositoriesPerRequest)).ConfigureAwait(false);

            return response.Data;

            static string getEndCursorString(string? endCursor) => string.IsNullOrWhiteSpace(endCursor) ? string.Empty : "after: \"" + endCursor + "\"";
        }
    }
}
