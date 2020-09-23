using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GitHubReadmeWebTrends.Common
{
    public class GitHubGraphQLApiService
    {
        readonly IGitHubGraphQLApiClient _gitHubGraphQLApiClient;

        public GitHubGraphQLApiService(IGitHubGraphQLApiClient gitHubGraphQLApiClient) => _gitHubGraphQLApiClient = gitHubGraphQLApiClient;

        public Task<CreateBranchResponseModel> CreateBranch(string repositoryId, string repositoryName, string branchOid, Guid guid) =>
            ExecuteGraphQLRequest(_gitHubGraphQLApiClient.CreateBranchMutation(new CreateBranchMutationContent(repositoryId, repositoryName, branchOid, guid)));

        public Task<RepositoryConnectionResponse> GetRepository(string repositoryOwner, string repositoryName) =>
            ExecuteGraphQLRequest(_gitHubGraphQLApiClient.RepositoryConnectionQuery(new RepositoryConnectionQueryContent(repositoryOwner, repositoryName)));

        public Task<CreatePullRequestResponseModel> CreatePullRequest(in string repositoryId, in string baseRefName, in string headRefName, in string title, in string body, in Guid clientMutationId, in bool maintainerCanModify = true, in bool draft = false) =>
            ExecuteGraphQLRequest(_gitHubGraphQLApiClient.CreatePullRequestMutation(new CreatePullRequestMutationContent(repositoryId, baseRefName, headRefName, title, body, clientMutationId, maintainerCanModify, draft)));

        public Task<GitHubViewerResponse> GetViewerInformation() => ExecuteGraphQLRequest(_gitHubGraphQLApiClient.ViewerLoginQuery(new ViewerLoginQueryContent()));

        public async IAsyncEnumerable<IEnumerable<Repository>> GetRepositories(string repositoryOwner, int numberOfRepositoriesPerRequest = 100)
        {
            RepositoriesConnectionResponse? repositoryConnection = null;

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
                throw new AggregateException(response.Errors.Select(x => new Exception(x.Message)));
            else if (response.Errors != null && response.Errors.Any())
                throw new Exception(response.Errors.First().Message.ToString());

            return response.Data;
        }

        Task<RepositoriesConnectionResponse> GetRepositoryConnectionResponse(string repositoryOwner, string? endCursor, int numberOfRepositoriesPerRequest = 100)
        {
            return ExecuteGraphQLRequest(_gitHubGraphQLApiClient.RepositoriesConnectionQuery(new RepositoriesConnectionQueryContent(repositoryOwner, getEndCursorString(endCursor), numberOfRepositoriesPerRequest)));

            static string getEndCursorString(string? endCursor) => string.IsNullOrWhiteSpace(endCursor) ? string.Empty : "after: \"" + endCursor + "\"";
        }
    }
}
