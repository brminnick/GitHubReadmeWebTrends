using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VerifyGitHubReadmeLinks
{
    class GitHubGraphQLApiService
    {
        readonly IGitHubGraphQLApiClient _gitHubGraphQLApiClient;

        public GitHubGraphQLApiService(IGitHubGraphQLApiClient gitHubGraphQLApiClient) => _gitHubGraphQLApiClient = gitHubGraphQLApiClient;

        public Task<CreateBranchResponse> CreateBranch(string repositoryId, string repositoryName, string branchOid, Guid guid) =>
            ExecuteGraphQLRequest(_gitHubGraphQLApiClient.CreateBranch(new CreateBranchMutationContent(repositoryId, repositoryName, branchOid, guid)));

        public async IAsyncEnumerable<IEnumerable<Repository>> GetRepositories(string repositoryOwner, int numberOfRepositoriesPerRequest = 100)
        {
            RepositoryConnectionResponse? repositoryConnection = null;

            do
            {
                repositoryConnection = await GetRepositoryConnectionResponse(repositoryOwner, repositoryConnection?.PageInfo?.EndCursor, numberOfRepositoriesPerRequest).ConfigureAwait(false);
                yield return repositoryConnection?.RepositoryList ?? Enumerable.Empty<Repository>();
            }
            while (repositoryConnection?.PageInfo?.HasNextPage is true);
        }

        static async Task<T> ExecuteGraphQLRequest<T>(Task<GraphQLResponse<T>> graphQLRequestTask)
        {
            var response = await graphQLRequestTask.ConfigureAwait(false);

            if (response.Errors != null && response.Errors.Count() > 1)
                throw new AggregateException(response.Errors.Select(x => new Exception(x.ToString())));
            else if (response.Errors != null && response.Errors.Any())
                throw new Exception(response.Errors.First().ToString());

            return response.Data;
        }

        Task<RepositoryConnectionResponse> GetRepositoryConnectionResponse(string repositoryOwner, string? endCursor, int numberOfRepositoriesPerRequest = 100)
        {
            return ExecuteGraphQLRequest(_gitHubGraphQLApiClient.RepositoryConnectionQuery(new RepositoryConnectionQueryContent(repositoryOwner, getEndCursorString(endCursor), numberOfRepositoriesPerRequest)));

            static string getEndCursorString(string? endCursor) => string.IsNullOrWhiteSpace(endCursor) ? string.Empty : "after: \"" + endCursor + "\"";
        }
    }
}
